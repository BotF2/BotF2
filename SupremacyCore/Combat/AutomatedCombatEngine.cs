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
        private int[,] empiresInBattle;

        public AutomatedCombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {
            empiresInBattle = new int[12, 3];
        }
        protected override void ResolveCombatRoundCore()
        {
            
            GameLog.Core.CombatDetails.DebugFormat("_combatShips.Count: {0}", _combatShips.Count());
            bool activeBattle = true; // false when less than two civs remaining
            // Scouts, Frigate and cloaked ships have a special chance of retreating BEFORE round 3
            if (_roundNumber < 7) // multiplayer starts at round 5
            {
                GameLog.Core.CombatDetails.DebugFormat("round# ={0} now", _roundNumber);
                //  Once a ship has retreated, its important that it does not do it again..
                var easyRetreatShips = _combatShips
                    .Where(s => s.Item1.IsCloaked == true || (s.Item1.Source.OrbitalDesign.Key.Contains("FRIGATE")) || (s.Item1.Source.OrbitalDesign.ShipType == "Scout"))
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
                            GameLog.Core.CombatDetails.DebugFormat("Easy retreated ={0}", ship.Item1.Name);
                            //SendUpdates();
                        }
                    }
                }
                // other ships with retreat order have a lesser chance to retreat
                var hardRetreatShips = _combatShips
                    .Where(s => s.Item1.IsCloaked != true && !s.Item1.Source.OrbitalDesign.Key.Contains("FRIGATE") && s.Item1.Source.OrbitalDesign.ShipType != "Scout")
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
                            GameLog.Core.CombatDetails.DebugFormat("Hard retreated ={0}", ship.Item1.Name);
                        }
                    }
                }
                //Decloak any cloaked ships 
                foreach (var combatShip in _combatShips)
                {
                    if (combatShip.Item1.IsCloaked)
                    {
                        combatShip.Item1.Decloak();
                        GameLog.Core.CombatDetails.DebugFormat("Ship  {0} {1} ({2}) cloak status {3})",
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
                                ownerAssets.AssimilatedShips.Add(target.Item1);
                                _combatShips.Remove(target);
                                GameLog.Core.CombatDetails.DebugFormat("Assimilated ={0} found borg ={1} assimilationSuccessful ={2}, chance to Assimiate ={3}", target.Item1.Name, foundDaBorg, assimilationSuccessful, chanceToAssimilate);
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
            int q = 0;
            foreach (int ownerID in ownerIDs.Distinct())
            {
                empiresInBattle[q, 0] = ownerID;

                // CHANGE X PROBLEM: /// 777 means computer has no target. 888 means return fire for human or nothing checked. target 2 is always 777
                // If target 2 = lurian (85), 777 is returned. If i click both, target 1 = 85, target 2 = 777
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
                {   // Update X 03 july 2019 change 888 to 777
                    if (empiresInBattle[q, 1] == 777 && empiresInBattle[q, 2] == 777) /// 777 = No target choosen. 888 = Vuluntarily not firing
                    {
                        empiresInBattle[q, 1] = 999; // 999 = null = no active fire)
                        empiresInBattle[q, 2] = 999;
                    }
                    // UPDATE X 03 july 2019 changed back to 888, meaning if one target was not filled, both targets are set to the one that was
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
                    // deleted: (empiresInBattle[q, 1] == 777 || empiresInBattle[q, 1] == 999) &&
                    if ( (civi.DiplomacyReport.Contains("Warlike") || civi.DiplomacyReport.Contains("Hostile")))
                    {
                        while (true)
                        {
                            if (allparticipatings.Count() < 2)
                            {
                                activeBattle = false;
                                break;
                            }      
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
                        empiresInBattle[q, 1] = 888; // Return Fire for all AIs, if no other war etc.
                    }
                    // UPDATE X 25 june 2019 added if == 999 & warlike then choose a random target. + Minichange, from DiplomacyReport back to Traits
                    // delete (empiresInBattle[q, 2] == 777 || empiresInBattle[q, 2] == 999) && (
                    if (civi.Traits.Contains("Warlike") || civi.Traits.Contains("Hostile"))
                    {
                        while (true)
                        {
                            if (allparticipatings.Count() < 2)
                            {
                                activeBattle = false;
                                break;
                            }
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
                        empiresInBattle[q, 2] = 888; // Return Fire for all AIs, if no other war etc.
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
                GameLog.Core.CombatDetails.DebugFormat("Empire Civ in Battle: {0} FirstTarget = {1} 2nd Target = {2}, Civ q = {3}", empiresInBattle[q, 0], empiresInBattle[q, 1], empiresInBattle[q, 2], q);
                q = q + 1;
                //    GameLog.Core.CombatDetails.DebugFormat("Empire Civ in Battle: {0} FirstTarget = {1} 2nd Target = {2}", empiresInBattle[q, 0], empiresInBattle[q, 1], empiresInBattle[q, 2]);
            }
            #endregion
            foreach (var item in ownerIDs)
            {
                GameLog.Core.CombatDetails.DebugFormat("ownerIDs contains = {0}", item);
            }

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
            // Amount of Firepower the other Empire had. Its the base for return fire
            // loops from one empire attacking (and recieving return fire) to the next, until all ships have fired

            // Counts during 2nd loop (Attacking Loop, how many runs there where)

            //int shipCount = 0;
            //for (int i = 0; i <11; i++)
            //{
            //    var testforShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == empiresInBattle[i, 0])
            //           .Where(sc => sc.Item1.ReminingFirePower > 0).Select(sc => sc).ToList();
            //    if(testforShips == null)
            //    {

            //    }
            //    else
            //    {
            //        shipCount += 1;
            //    }
            //}
            //if (shipCount < 2)
            //{
            //    activeBattle = false;
            //}
            int AttackingEmpireID = 999;
            int targetedEmpireID = 999;


            while (activeBattle == true)
            {
                GameLog.Core.CombatDetails.DebugFormat("--------");
                int attackingRoundCounts = 0;
                int countReturnFireLoop = 0;
                int returnFireFirepower = 0;

                if (TargetOneORTwo == 3) // if trying to attack target three (not available), target empire one again
                {
                    TargetOneORTwo = 1;
                }


                // works   GameLog.Core.CombatDetails.DebugFormat("Current Target One or Two? in Main While {0} ", TargetOneORTwo);
                AttackingEmpireID = empiresInBattle[indexOfAttackerEmpires, 0];
                targetedEmpireID = empiresInBattle[indexOfAttackerEmpires, 0 + TargetOneORTwo];
                // CHANGE X (switched AttackingEmpireID and targetedEmpireID)
                int ReturnFireEmpire = targetedEmpireID;
                int EmpireAttackedInReturnFire = AttackingEmpireID;
                // Let Empire One (which is in empiresInBattle[0,0]) fire first
                // Search for the next fitting ship, that is of the targeted empire AND has Hull > 0

                var AttackingShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                    .Where(sc => sc.Item1.RemainingFirepower > 0).Select(sc => sc).ToList();
                var AttackingShip = AttackingShips.FirstOrDefault();
                if (AttackingShips.Count > 0 && AttackingShips != null) // UPDATE 07 july 2019 make sure it does not crash, use count >0
                {
                    AttackingShip = AttackingShips.RandomElementOrDefault();
                   GameLog.Core.Combat.DebugFormat("Current Attacking Ship {0}", AttackingShip.Item1.Name);
                }
                // COUNT ACTICE FIREROUND PER EMPIRE
                shipsPerEmpire[indexOfAttackerEmpires, 2] += 1;
                if (shipsPerEmpire[indexOfAttackerEmpires, 2] > 12) // CHNAGE X More fireingrounds
                {
                    if (shipsPerEmpire[indexOfAttackerEmpires, 2] > shipsPerEmpire[indexOfAttackerEmpires, 1] * 0.9)
                    {
                        AttackingShip = null;
                    }
                }
                if (targetedEmpireID == 999 || targetedEmpireID == 777 || targetedEmpireID == 888) // UPDATE X 10 july 2019 added 888
                {
                    AttackingShip = null; // refue to fire activly, if user / AI sais so
                }

                // works    GameLog.Core.CombatDetails.DebugFormat("Index of current Attacker Empire {0}", AttackingEmpireID);

                if (AttackingShip is null) // either because they cannot, or they refuse to fire activly. // CHANGE X test
                {
                    indexOfAttackerEmpires += 1; // give the next Empire a try
                    if (empiresInBattle[indexOfAttackerEmpires, 0] == 999)
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
                    {
                        continue;
                    }
                }
                else
                {
                    howOftenContinued = 0;

                    returnFireFirepower = AttackingShip.Item1.Firepower; // Tranfers Empire´s Attacking Ship Total Firepower to be the base for the other Empire return fire.
                }
                //END NEW123
                GameLog.Core.CombatDetails.DebugFormat("Saved returnFirepower later used in next loop {0}", returnFireFirepower);
                double ScissorBonus = 0d; // This adds a bonus e.g. if a destroyer is firing on a command ship
                int remainingFirepowerInWhile = 0; // Counts if there is remaining firepower that would hit another ship, too.
                /// NEW123
                bool additionalRun = false; // addtional run  -> more targets
                // END NEW123
                // Attacking Ship looks for target(s)
                GameLog.Core.CombatDetails.DebugFormat("Loop for finding an Target(s) for Attacking Ship starts");
                #endregion

                double FavorTheBoldAttackBonus = 1.0;

                // Update X 18 august Flavor the bold modfiier
                // Calculate and save EmpireDurability for later use in Flavor the Bold
                int[,] EmpireTotalDurabilities;
                EmpireTotalDurabilities = new int[12, 2];
                
                // Initialize Array
                for(int i = 0; i < 12; i++ )
                {
                    EmpireTotalDurabilities[i, 0] = 999;
                    EmpireTotalDurabilities[i, 1] = 999;

                }

                int FleetStrenghtTemp = 0;
                for(int i = 0; i< 12; i++)
                {
                    // If no more empire found, break
                    if (empiresInBattle[i, 0] == 777 ||
                        empiresInBattle[i, 0] == 888 ||
                            empiresInBattle[i, 0] == 999)
                        break;

                    EmpireTotalDurabilities[i, 0] = empiresInBattle[i, 0];
                    
                    var fleet = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == empiresInBattle[i, 0])
                    .Select(sc => sc).ToList();

                    foreach (var ship in fleet)   // only combat ships 
                    {

                        FleetStrenghtTemp = FleetStrenghtTemp + Convert.ToInt32(ship.Item1.Firepower
                                + Convert.ToInt32(ship.Item1.ShieldStrength + ship.Item1.HullStrength)
                                * ((1 + Convert.ToDouble(ship.Item1.Source.OrbitalDesign.Maneuverability) / 0.24 / 100))
                            );

                        //GameLog.Core.CombatDetails.DebugFormat("adding _hostileEmpireStrength for {0} {1} ({2}) = {3} - in total now {4}",
                        //    cs.Source.ObjectID, cs.Source.Name, cs.Source.Design, cs.FirePower, _hostileEmpireStrength);
                    }

                    if (_combatStation != null)
                    { 
                    if (_combatStation.Item1.OwnerID == EmpireTotalDurabilities[i, 0])
                    {
                        FleetStrenghtTemp = FleetStrenghtTemp + _combatStation.Item1.Firepower + _combatStation.Item1.HullStrength + _combatStation.Item1.ShieldStrength;

                    }
                    }

                    EmpireTotalDurabilities[i, 1] = FleetStrenghtTemp;
                    FleetStrenghtTemp = 0;
                
                }
                



                #region attacker loop




                // HERE STARTS ATTACKER´S LOOP LOOKING FOR TARGETS
                GameLog.Core.CombatDetails.DebugFormat("NOW HERE ATTACKING LOOP STARTS!");
                while (true) // Attacking Ship looks for target(s) - all c# collections can be looped
                {
                    GameLog.Core.CombatDetails.DebugFormat("-----------------------");
                    attackingRoundCounts += 1;
                    GameLog.Core.CombatDetails.DebugFormat("In Attacking Round {0}, the EmpireID {1} is used", attackingRoundCounts, AttackingEmpireID);
                    int rememberForDamage = 0;
                    if (targetedEmpireID == 999 || targetedEmpireID == 777 || targetedEmpireID == 888) // UPDATE X 8 july 2019 in all 3 cases no active attack
                    {
                        GameLog.Core.CombatDetails.DebugFormat("Loop for finding an Target(s) for Attacking Ship starts BREAKS, becasue Human/AI has no target selected");
                        break; // refue to fire activly, if user / AI sais so
                    }
                    //var defenderOrder = CombatOrder.Retreat; // default order for now when 'target' is a dummy civilization
                    var currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                            .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                    var currentTarget = currentTargets.FirstOrDefault(); // Also make it distinct
                    if (currentTargets != null && currentTargets.Count >0) // UPDATE 07 july 2019 make sure it does not crash, use count >0
                    {
                        currentTarget = currentTargets.RandomElementOrDefault();
                    }
                    if (currentTarget == null || currentTargets.Count == 0) // UPDATE 07 july 2019 make sure it does not crash, use count >0
                    {
                        if (attackingRoundCounts == 1)
                        {
                            returnFireFirepower = 0; // If no target found at all (round = 1) then no retaliation, because there is no retaliation ship anyway and there was no damage applied as well
                        }
                        if (_combatStation != null)
                        {
                            if (_combatStation.Item1.OwnerID == targetedEmpireID && _combatStation.Item1.HullIntegrity > 0) // Update x 18 august 2019 added ? hullstrenght >0 
                            {
                                currentTarget = _combatStation;
                            }
                        }
                        else
                        {            // NO (MORE) TARGET. Save attackingships (remaining) Weapons
                            if (remainingFirepowerInWhile > 0)
                            {
                                // Remaining Firepower is only set just after fireing
                                // let this AttackShip "RemainingFirepower" be returnFireFirepower
                                //AttackingShip.Item1.RemainingFirepower = remainingFirepowerInWhile;
                                GameLog.Core.CombatDetails.DebugFormat("No more target found in AttackingLoop. Trying to update for ship Name: {0} with remaining firepower = {1}", AttackingShip.Item1.Name, remainingFirepowerInWhile);
                                //var testAttackingShip = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                //        .Where(sc => sc.Item1.RemainingFirepower > 0).Select(sc => sc).ToList();
                                break;
                                // use Gamelog/test that ship needs to have reduced weapons in _combatShipsTemp
                            }
                            else
                            {
                                //AttackingShip.Item1.RemainingFirepower = 0;//remainingFirepowerInWhile;
                                //GameLog.Core.CombatDetails.DebugFormat("No more target found in AttackingLoop. Trying to update for ship Name: {0} with remaining firepower = {1}", AttackingShip.Item1.Name, remainingFirepowerInWhile);
                                //var testAttackingShip = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                //        .Where(sc => sc.Item1.RemainingFirepower > 0).Select(sc => sc).ToList();
                                GameLog.Core.CombatDetails.DebugFormat("Loop for finding an Target(s) for Attacking Ship starts BREAKS because no target found");
                                //if (testAttackingShip != null)
                                //{

                                //}
                                break; // VERY NEW
                            }

                        }
                        // If target is null or == 0
                        GameLog.Core.CombatDetails.DebugFormat("Coudn´t find a target in attacker run. BREAK");
                        break;
                    }

                    

                    // FAVOR THE BOLD
                    // UPDATE X 18 August 2019

                    int AttackingDurabilityTotal = 0;
                    int DefendingDurabilityTotal = 0;

                    for (int i2 = 0; i2 < 12; i2++)                   
                    {
                        // If no more empire found
                        if (EmpireTotalDurabilities[i2, 0] == 999)
                            break;

                        if (EmpireTotalDurabilities[i2,0] == Convert.ToInt32(AttackingShip.Item1.OwnerID))
                        {
                            AttackingDurabilityTotal = EmpireTotalDurabilities[i2, 1];
                        }
                        if (EmpireTotalDurabilities[i2, 0] == Convert.ToInt32(currentTarget.Item1.OwnerID))
                        {
                            DefendingDurabilityTotal = EmpireTotalDurabilities[i2, 1];
                        }

                    }

                    if (AttackingDurabilityTotal * 2 < DefendingDurabilityTotal
                        )
                    {

                    
                        if (GameContext.Current.TurnNumber <10)
                        {
                            if (AttackingDurabilityTotal > 1000)
                                FavorTheBoldAttackBonus = 1.1;
                        }
                        else if (GameContext.Current.TurnNumber >= 10)
                        {
                            if (AttackingDurabilityTotal > 4000)
                                FavorTheBoldAttackBonus = 1.2;
                            if (GameContext.Current.TurnNumber > 30)
                            {
                                if (AttackingDurabilityTotal > 7000)
                                    FavorTheBoldAttackBonus = 1.3;
                            }
                            if (GameContext.Current.TurnNumber > 60)
                            {
                                if (AttackingDurabilityTotal > 14000)
                                    FavorTheBoldAttackBonus = 1.4;
                            }
                            if (GameContext.Current.TurnNumber > 85)
                            {
                                if (AttackingDurabilityTotal > 28000)
                                    FavorTheBoldAttackBonus = 1.5;
                            }
                            if (GameContext.Current.TurnNumber >120)
                            {
                                if (AttackingDurabilityTotal > 40000)
                                    FavorTheBoldAttackBonus = 1.6;
                            }
                             if (GameContext.Current.TurnNumber > 160)
                            {
                                if (AttackingDurabilityTotal > 60000)
                                 FavorTheBoldAttackBonus = 1.7;
                            }
                        }

                    }
                    else
                    {
                        // If attacker outguns the targeted empire, the attacked gain less damage
                        if(AttackingDurabilityTotal > DefendingDurabilityTotal * 2)
                            {
                               if (GameContext.Current.TurnNumber <20)
                                    FavorTheBoldAttackBonus = 0.9;
                            }
                            else
                            {
                                if (GameContext.Current.TurnNumber >= 20)
                                {
                                    FavorTheBoldAttackBonus = 0.85;
                                }
                                if (GameContext.Current.TurnNumber > 60)
                                {
                                    FavorTheBoldAttackBonus = 0.8;
                                }
                                if (GameContext.Current.TurnNumber > 120)
                                {
                                    FavorTheBoldAttackBonus = 0.7;
                                }
                                if (GameContext.Current.TurnNumber > 200)
                                {
                                    FavorTheBoldAttackBonus = 0.6;
                                }
                            }

                        }


                        if (AttackingDurabilityTotal > DefendingDurabilityTotal *10)
                        {

                            FavorTheBoldAttackBonus = 1.3;
                        }
                        else if (DefendingDurabilityTotal > AttackingDurabilityTotal *10)
                        {
                            FavorTheBoldAttackBonus = 0.9;
                        }

                        /// FavorTheBoldAttackBonus needs to be used in damage


                //if (currentTarget is null || currentTargets.Count == 0) // UPDATE 07 july 2019 make sure it does not crash, use count >0
                //{
                //    GameLog.Core.CombatDetails.DebugFormat("current Target is: (for Attacking loop) NONE, BREAK");
                //    break;
                //}
                //else
                //{
                //    GameLog.Core.CombatDetails.DebugFormat("current Target is: (for Attacking loop){0}", currentTarget.Item1.Name);
                //}
                var attackerOrder = GetCombatOrder(AttackingShip.Item1.Source);
                    var defenderOrder = GetCombatOrder(currentTarget.Item1.Source);
                    if (defenderOrder.ToString() == null || attackerOrder.ToString() == null)
                    {
                        GameLog.Core.CombatDetails.DebugFormat("Warning. defender OR attackerOrder == null, in Attackerloop");
                    }
                    // UPDATE 07 july 2019 make sure it does not crash, use count >0
                    if ((_combatStation != null && currentTargets.Count > 0) && defenderOrder != CombatOrder.Formation) // Formation protects Starbase, otherwise ships are protected.
                    {
                        if (_combatStation.Item1.OwnerID == targetedEmpireID)
                        {
                            if (_combatStation.Item1.HullIntegrity > 0) // is this how to get int our of HullStrength Meter?
                            {
                                currentTarget = _combatStation; // Station in _combatShips
                            }
                        }
                    }
                    if (currentTargets.Count > 0)
                    {
                        
                        GameLog.Core.CombatDetails.DebugFormat("Still Attacking loop: Change target to (Station if station owner is not formation) and station rpesent. (new) target: {0}", currentTarget.Item1.Name);
                    }
                    else // UPDATE 07 july 2019 make sure it does not crash, use count >0
                    {
                        if (attackingRoundCounts == 1) // Copy of above if trying to find other targets (transports, stations, frigates) and nwo there is a problem... and its still the first try
                        {
                            returnFireFirepower = 0; // If no target found at all (round = 1) then no retaliation, because there is no retaliation ship anyway and there was no damage applied as well
                        }

                        // ReturnFirePower stays, if it is round 2 therefore damage was applied and other side has had some target(s)
                        GameLog.Core.CombatDetails.DebugFormat("Still Attacking loop: No more targets");
                    }
                        // Calculate Bonus/Malus
                        // Get Accuracy, Damage Control when fixed
                        // double sourceAccuracyTemp = 1; // used to determin whether or not it is a hit
                    double sourceAccuracy = 0.70; // used to increase damage as well, if hero ship
                    double targetDamageControl = 0.55;

                    // COMMAND SHIP MODFIER (ATTACKING LOOP)
                    double commandShipModifierAccuracy = -0.15;
                    var ammountCommandShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                        .Where(sc => sc.Item1.HullStrength > 0)
                                        .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType == "Command" || sc.Item1.Source.OrbitalDesign.Key.Contains("BATTLESHIP"))
                                        .Select(sc => sc).ToList();
                    var totalammount = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                            .Where(sc => sc.Item1.HullStrength > 0)
                                            .Select(sc => sc).ToList();

                    if (ammountCommandShips != null && totalammount != null && ammountCommandShips.Count > 0 && totalammount.Count > 0)
                    {
                        if (ammountCommandShips.Count > 0)
                        {
                            commandShipModifierAccuracy = 0.01;
                        }
                        if(ammountCommandShips.Count >= totalammount.Count / 10)
                        {
                            commandShipModifierAccuracy = 0.10;
                        }
                    }

                    // CHECKING EXPERIENCE for accuracy and damage control (attacking loop)
                    // Checking ship experienc ein Attacking loop for the attacking ship (station)
                    string ShipExperience = "Unknown";
                    ShipExperience = AttackingShip.Item1.Source.ExperienceRankString;
                    switch (ShipExperience)
                    {

                        case "Unknown":
                            {
                                sourceAccuracy = 0.70;
                                //targetDamageControl = 0.75;
                                break;
                            }
                        case "Green":
                            {
                                sourceAccuracy = 0.50;
                                //targetDamageControl = 0.45;
                                break;
                            }
                        case "Regular":
                            {
                                sourceAccuracy = 0.60;
                                //targetDamageControl = 0.50;
                                break;
                            }
                        case "Veteran":
                            {
                                sourceAccuracy = 0.70;
                                //targetDamageControl = 0.55;
                                break;
                            }
                        case "Elite":
                            {
                                sourceAccuracy = 1;
                                //targetDamageControl = 0.65;
                                break;
                            }
                        case "Legendary":
                            {
                                sourceAccuracy = 1.1;
                                //targetDamageControl = 0.75;
                                break;
                            }
                    }
                    if (AttackingShip.Item1.Name.Contains("!"))
                    {
                        sourceAccuracy = sourceAccuracy + 0.80; // If attacking ship is also a hero ship add 0.8 accuracy.
                        
                    }
                    if (AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("CRUISER") || AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("DESTROYER")
                        || AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("FRIGATE") ||
                        (AttackingShip.Item1.Source.Owner.Name.Contains("Borg") &&
                        AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("SPHERE") ||
                        AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("DIAMOND") ||
                        AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("CUBE")
                        )
                        )
                    {
                        sourceAccuracy = sourceAccuracy + commandShipModifierAccuracy; // taking Command ship present or no present modfier into account.
                    }
                    else if (!AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("COMMAND")) // Civilian Non-Command ship only get negative mod
                    {
                        if(commandShipModifierAccuracy < 0) // only if modifyer is negative
                        {
                            sourceAccuracy = sourceAccuracy + commandShipModifierAccuracy; 
                        }
                    }

                    // update x 23 july 2019: added experience to change accuracy and damage control: Checking Target experience in Attacking Loop
                        string TargetShipExperience = "Unknown";
                            TargetShipExperience = currentTarget.Item1.Source.ExperienceRankString;
                            switch (TargetShipExperience)
                            {

                                case "Unknown":
                                    {
                                        //sourceAccuracy = 0.75;
                                        targetDamageControl = 0.55;
                                        break;
                                    }
                                case "Green":
                                    {
                                        //sourceAccuracy = 0.45;
                                        targetDamageControl = 0.40;
                                        break;
                                    }
                                case "Regular":
                                    {
                                        //sourceAccuracy = 0.50;
                                        targetDamageControl = 0.45;
                                        break;
                                    }
                                case "Veteran":
                                    {
                                        //sourceAccuracy = 0.55;
                                        targetDamageControl = 0.50;
                                        break;
                                    }
                                case "Elite":
                                    {
                                        //sourceAccuracy = 0.65;
                                        targetDamageControl = 0.57;
                                        break;
                                    }
                                case "Legendary":
                                    {
                                        //sourceAccuracy = 0.75;
                                        targetDamageControl = 0.65;
                                        break;
                                    }

                            }
                    if (currentTarget.Item1.Source.IsMobile == false) // Stations have higher damage control
                        targetDamageControl = targetDamageControl + 0.55;
                    if (currentTarget.Item1.Name.Contains("!"))
                    {
                        targetDamageControl = targetDamageControl + 0.55;
                    }
                    

                   
                    //  compare Orders
                    double combatOrderBonusMalus = 0;
                    // Engage
                    if (attackerOrder == CombatOrder.Engage && (defenderOrder == CombatOrder.Rush || defenderOrder == CombatOrder.Formation))
                        combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.RemainingFirepower * 0.15;
                    // RAID
                    if (attackerOrder == CombatOrder.Transports && defenderOrder != CombatOrder.Formation) // if Raid, and no Formation select Transportships to be targeted
                    {
                        combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.RemainingFirepower * 0.17;
                        currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                            .Where(sc => sc.Item1.HullStrength > 0)
                            .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType.Contains("Transport"))
                            .Select(sc => sc).ToList();
                        currentTarget = currentTargets.RandomElementOrDefault();
                        if (currentTarget is null) // No Transports found
                        {
                            currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                            currentTarget = currentTargets.RandomElementOrDefault(); // Find non-transport opponents
                        }
                        if (attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Engage)
                            combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.RemainingFirepower * 0.13; // even more weapon Bonus if defender is Engaging
                    }
                    else if ((attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Formation && _combatStation is null)) // IF Raiding and Defender is doing combat Formating, let Frigates protect Transports
                    {
                        currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                            .Where(sc => sc.Item1.HullStrength > 0)
                            .Where(sc => sc.Item1.Source.OrbitalDesign.Key.Contains("FRIGATE"))
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
                    if(currentTargets.Count == 0) // UPDATE 07 july 2019 make sure it does not crash, use count >0
                    {
                        GameLog.Core.CombatDetails.DebugFormat("Still Attacking loop: No more targets Found after checking transports/frigates etc. Break out of Attacking lopp");
                        
                        if (attackingRoundCounts == 1) // No target in round 1? then
                        {
                            returnFireFirepower = 0; // If no target found at all (round = 1) then no retaliation, because there is no retaliation ship anyway and there was no damage applied as well
                        }
                        // Else
                        // leave returnFirePower as iterator it is for return firepower
                        // SAVE REMAINING WEAPONS not nessecary no target, no attacker = no fire, = no lowering of firepower
                        break; // No targets found
                    }
                    // Rush
                    if (attackerOrder == CombatOrder.Rush && (defenderOrder == CombatOrder.Retreat || defenderOrder == CombatOrder.Transports))
                        combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.RemainingFirepower * 0.12;
                    // Formation
                    if (attackerOrder == CombatOrder.Formation && (defenderOrder == CombatOrder.Transports || defenderOrder == CombatOrder.Rush))
                        combatOrderBonusMalus = combatOrderBonusMalus + AttackingShip.Item1.RemainingFirepower * 0.17;
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
                        ScissorBonus = AttackingShip.Item1.RemainingFirepower * 0.35; // 20 % Scissor Bonus
                                                                                    // BOnus/Malus is applied to damage sum
                    GameLog.Core.CombatDetails.DebugFormat("follogwing Bonus/Malus a) due to Order: = {0}, b) due to Scissor = {1}", combatOrderBonusMalus, ScissorBonus);
                    // Do we have more Weapons then target has shields? FirepowerRemains... /// NEW123 added combatOrderBonusMallus and other changes // Maneuverability 8 = 33% more shields. 1 = 4% more shields
                    int check = currentTarget.Item1.Source.GetManeuverablility(); // allows to check if maneuverability is gotten correctly
                    if (additionalRun) // if its a  new run use remainingFirepowerinWhile insstead
                    {
                        GameLog.Core.CombatDetails.DebugFormat("We are in an addtiona´run (next target for attacking loop)");
                        // And new target can now aborb damage
                        if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * 
                            (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                        remainingFirepowerInWhile) // if remainingFirepower is absorbed by targets Hull/shields/Maneuverability, set it to -1 and discharge weapons.
                        {
                            GameLog.Core.CombatDetails.DebugFormat("this time (additional run) in this attacking loop the target absorbt all weapons");
                            remainingFirepowerInWhile = -1;
                            //foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire))
                            //{
                            //    weapon.Discharge();
                            //}
                            //AttackingShip.Item1.RemainingFirepower = 0;
                        } // Otherwise we have yet another run with remainingFirepowerinWhile
                        else
                        {
                            remainingFirepowerInWhile = remainingFirepowerInWhile
                              - Convert.ToInt32((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) *
                              (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24)) / 100);
                        }
                        // Otherwise we still have remainingFirepower The no -1 means we will get an addtional run
                    }
                    else
                    {
                        
                        // If first run and target can absorb full damage
                        if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * 
                            (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                            AttackingShip.Item1.RemainingFirepower)
                        {
                            rememberForDamage = Convert.ToInt32(AttackingShip.Item1.RemainingFirepower);  // Change X
                            remainingFirepowerInWhile = -1;
                            GameLog.Core.CombatDetails.DebugFormat("its the run on the first target in attacking loop and it can already absorb all weapons");
                            //foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire)) // Discharge Weapons
                            //{
                            //    weapon.Discharge();
                            //    AttackingShip.Item1.RemainingFirepower = 0; // Set remaining firepower to 0 // Uupdate X 10 july 2019 do not drain weapons before fireing
                            //}
                        }
                        else
                        {
                            // CHANGE X need to fix bonusstuff
                            remainingFirepowerInWhile = Convert.ToInt32(AttackingShip.Item1.RemainingFirepower) 
                                    - Convert.ToInt32((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) 
                                    * (1 + currentTarget.Item1.Source.GetManeuverablility() / 0.24 /100));
                            //remainingFirepowerInWhile = remainingFirepowerInWhile - Convert.ToInt32((Convert.ToDouble(remainingFirepowerInWhile));
                            GameLog.Core.CombatDetails.DebugFormat("its the first run on a target, an weapons remain RemainingFirepowerInWhile == {0}", remainingFirepowerInWhile);
                        }
                    }
                    // Fire Weapons, inflict damage. Either with all calculated bonus/malus. Or if this was done last turn, use remainingFirepower (if any)
                    Random zufall = new Random();
                    double tempDamage = 0;
                    if (remainingFirepowerInWhile == 1)
                    {
                        currentTarget.Item1.TakeDamage((int)(Convert.ToInt32(tempDamage= 
                            (rememberForDamage + Convert.ToInt32(ScissorBonus)
                            + Convert.ToDouble(combatOrderBonusMalus)
                            * Convert.ToDouble(1.5 - targetDamageControl)
                            * Convert.ToDouble(FavorTheBoldAttackBonus)
                            * Convert.ToDouble(1 - currentTarget.Item1.Source.GetManeuverablility() / 0.24 / 100)
                            * sourceAccuracy * (zufall.Next(8,13)/10)
                            ))));// * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus)
                        AttackingShip.Item1.RemainingFirepower = 0;
                    }
                    else
                    {
                        currentTarget.Item1.TakeDamage((int)(Convert.ToInt32(tempDamage =
                            (AttackingShip.Item1.RemainingFirepower + Convert.ToInt32(ScissorBonus)
                            + Convert.ToDouble(combatOrderBonusMalus)
                            * Convert.ToDouble(1.5 - targetDamageControl)
                            * Convert.ToDouble(FavorTheBoldAttackBonus)
                            * Convert.ToDouble(1 - currentTarget.Item1.Source.GetManeuverablility() / 0.24 / 100)
                            * sourceAccuracy * (zufall.Next(8, 13) / 10)
                            )))); // minimal damage of 50 included
                        AttackingShip.Item1.RemainingFirepower = remainingFirepowerInWhile;

                    }
                    GameLog.Core.CombatDetails.DebugFormat("In Attacking Round {0}, the EmpireID {1} just fired", attackingRoundCounts, AttackingEmpireID);
                    GameLog.Core.CombatDetails.DebugFormat("now damage has just been applies either full weapons  (excluding bonus)  {0} OR lower damage if ship can only absorb that {1}", tempDamage, tempDamage);
                    GameLog.Core.CombatDetails.DebugFormat("Target has hull left {0}", currentTarget.Item1.HullStrength);
                    ////weapon.Discharge(); needed yes or no?
                    //END NEW123
                    if (remainingFirepowerInWhile == -1)
                    {
                        GameLog.Core.CombatDetails.DebugFormat("No more weapons on the attacking ship (loop), so no more run, break");
                        // Set AttackingShips TotalWeapons to 0
                        //NEW123
                        additionalRun = false; // Remebers if next run is FirstTargetRun OR...
                        break;
                    }// break the while, we do not need more targets for this AttackingShip
                    else
                    { // More Weapons available, continue for more targets 

                        GameLog.Core.CombatDetails.DebugFormat("Attacker has more weapons, an additional run is done to get more targets: {0}", remainingFirepowerInWhile);
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
                //remainingFirepowerInWhile = 0;

                targetedEmpireID = EmpireAttackedInReturnFire; // The guy who attacked is now the target
                AttackingEmpireID = ReturnFireEmpire; // Now the empire that was the targed will return fire
                bool needAdditionalAttackingShip = false; // Do we need an atttacking ship?
                if (returnFireFirepower > 0) // Yes if there is firepower to return
                    needAdditionalAttackingShip = true;
                int applyDamage = 0;

                #region returning fire loop
                // Here comes the next WHILE
                // HERE STARTS RETURNING FIRE LOOP
               // countReturnFireLoop += 1;
                // Now the attacked Empire returns fire, until same damage is dealed to attacking empire´s ship(s)
                GameLog.Core.CombatDetails.DebugFormat("Here starts RETURN FIRE LOOP TOtal Round: {0} if return fire >0: {1} ", countReturnFireLoop, returnFireFirepower);
                while (needAdditionalAttackingShip || additionalRun) // Either if we need an additional Attacking Ship to fire OR we have one and it needs to fire on more targets
                {
                    countReturnFireLoop += 1;
                    GameLog.Core.CombatDetails.DebugFormat("Loop for finding an Target(s) for Attacking Ship IN RETURN FIRE HAS STARTED (AGAIN) its loop {0}", countReturnFireLoop);
                    if (needAdditionalAttackingShip)
                    {
                        needAdditionalAttackingShip = false; 
                        AttackingShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                        .Where(sc => sc.Item1.RemainingFirepower > 0).Select(sc => sc).ToList();
                        if (AttackingShips != null)
                        {
                            AttackingShip = AttackingShips.RandomElementOrDefault();
                            // Update 21 july 2019, if station of same ID there, use Station as "AttackingShiP"
                            if (_combatStation != null)
                            {
                                if (AttackingEmpireID == _combatStation.Item1.OwnerID) // 23 july bugfix null^reference for ships
                                    AttackingShip = _combatStation;
                            }
                        }
                        else
                        {
                            if (_combatStation != null) // 21 july 2019
                            {
                                if (AttackingEmpireID == _combatStation.Item1.OwnerID)
                                    AttackingShip = _combatStation;
                            }
                        }
                        if (AttackingShip is null || AttackingShips.Count == 0)
                        {
                            if (countReturnFireLoop == 1) // If you do not even have one attacking ship in the first round, you need to forget about return fier
                            { 
                            returnFireFirepower = 0;
                            break; // stopp all return fire if we don´t have ships/stations to fire
                            }
                            break; // we cannot fire more because of lack of attacking ships
                        }
                    }

                    GameLog.Core.CombatDetails.DebugFormat("First Attacking Ship for RETURN FIRE found {0}", AttackingShip.Item1.Name);
                    // If AttackingShip can supply the required Weapons, we don´t need another attacking ship
                    if (returnFireFirepower < AttackingShip.Item1.RemainingFirepower) // This ship can close retaliation, has remaining firepower = Remaining - applyDamage
                    {
                        needAdditionalAttackingShip = false;  // Do we need more targets? Maybe, see way below
                        applyDamage = returnFireFirepower;
                        returnFireFirepower = 0; // Indicator 
                        GameLog.Core.CombatDetails.DebugFormat("First Attacking Ship has enought weapons to fully RETALIATE {0}", AttackingShip.Item1.Name);
                    }
                    else // we need another attacking ship, later, for the remaining returnFireFirepower
                    {
                        needAdditionalAttackingShip = true;
                        applyDamage = AttackingShip.Item1.RemainingFirepower;
                        returnFireFirepower = returnFireFirepower - applyDamage; // Next Ship needs to applay returnFireFirepower
                        GameLog.Core.CombatDetails.DebugFormat("Need more ships to apply full retailiation firepower: firepower left: {0}, applied first: {1}", returnFireFirepower, applyDamage);
                    }
                    // Getting a target // HEREX
                    
                    
                        var currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                                    .Where(sc => sc.Item1.HullStrength > 0).Select(sc => sc).ToList();
                        var currentTarget = currentTargets.RandomElementOrDefault(); // Also make it distinct
                        if (currentTarget != null && currentTargets.Count != 0)
                            GameLog.Core.CombatDetails.DebugFormat("We found a ship to be targeted: {0} to retaliate", currentTarget.Item1.Name);
                    
                        if (currentTarget == null || currentTargets.Count == 0)
                        {
                            if (_combatStation != null && currentTargets.Count != 0)
                            {
                                if (_combatStation.Item1.OwnerID == targetedEmpireID && _combatStation.Item1.HullIntegrity > 0)
                                {
                                    currentTarget = _combatStation;
                                }
                                else // VERY NEW
                                {
                                    additionalRun = false; // We cannot have an addtional run at another target because we ran already out of targets.
                                    if (countReturnFireLoop == 1)
                                    {
                                        GameLog.Core.CombatDetails.DebugFormat("No target in first retaliation run. Haven´t applied damaged. Havend fired, so nothing to set. BREAK");
                                        break;
                                    }
                                    else // We applied damage, and try again to find another target, but coudn´t. so save the remaining firepower to current ship.
                                         //if (returnFireFirepower > 0)
                                    {
                                        // let this AttackShip "RemainingFirepower" be returnFireFirepower
                                        // AttackingShip.Item1.RemainingFirepower = returnFireFirepower + applyDamage; // The Remaining FIrepower + the current not-applicable firepower (applyDamage)
                                        GameLog.Core.CombatDetails.DebugFormat(" In an additional run, no target. but still firepower to retaliate =  {0}, which is not used {1} BREAK", returnFireFirepower, AttackingShip.Item1.Name);
                                        // use Gamelog/test that ship needs to have reduced weapons in _combatShipsTemp
                                        break;
                                    }
                                    //else
                                    //{ No need because set to 0 happens when ships schields/hull >0 down there
                                    //    AttackingShip.Item1.RemainingFirepower = 0;
                                    //    GameLog.Core.CombatDetails.DebugFormat("Warning. no target for RETALIATIONloop. BREAK");
                                    //    GameLog.Core.CombatDetails.DebugFormat("We have no more targets, AND no more firepower to retaliate =  {0} BREAK", returnFireFirepower);
                                    //    break;
                                    //}
                                }
                            }
                            break; // No target, No Station. Break. We wanted most likly to retaliate more, but we´ve destroyed everyone. 
                        }
                        else
                        {
                            GameLog.Core.CombatDetails.DebugFormat("Found a target for retaliation {0}", currentTarget.Item1.Name);
                        }
                    
                    // Prepare and apply Bonuses/Maluses

                    var attackerOrder = GetCombatOrder(AttackingShip.Item1.Source);
                    //if (currentTarget is null  && currentTargets.Count == 0)
                    //{
                    //    GameLog.Core.CombatDetails.DebugFormat("Warning. no target for RETALIATIONloop. BREAK");
                    //    break;
                    //}
                    var defenderOrder = GetCombatOrder(currentTarget.Item1.Source);
                    
                    if ((_combatStation != null && currentTargets.Count != 0) && defenderOrder != CombatOrder.Formation) // Formation protects Starbase, otherwise ships are protected.
                    {
                        if (_combatStation.Item1.Source.HullStrength.CurrentValue > 0 && _combatStation.Item1.OwnerID == targetedEmpireID)
                        {
                            currentTarget = _combatStation;
                            GameLog.Core.CombatDetails.DebugFormat("Retaliation target has become station");
                        }
                    }

                    // DO WE NEED MORE TARGETs?
                    if(((currentTarget.Item1.HullStrength + currentTarget.Item1.ShieldStrength)+ 
                        ((currentTarget.Item1.HullStrength + currentTarget.Item1.ShieldStrength)*currentTarget.Item1.Source.GetManeuverablility()/0.24/100)) 
                        < applyDamage) // Target is destroyed before attacking ship can apply all damage
                    {
                        additionalRun = true; // So we need more targets (and maybe more attacking ships, maybe not)

                    }



                    // Calculate Bonus/Malus
                    // Get Accuracy, Damage Control when fixed
                    double sourceAccuracy = 0.70; 
                    double targetDamageControl = 0.55;

                    // COMMAND SHIP MODFIER (retaliation loop)
                    double commandShipModifierAccuracy = -0.15;
                    var ammountCommandShips = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                            .Where(sc => sc.Item1.HullStrength > 0)
                                            .Where(sc => sc.Item1.Source.OrbitalDesign.ShipType == "Command" || sc.Item1.Source.OrbitalDesign.Key.Contains("BATTLESHIP"))
                                            .Select(sc => sc).ToList();
                    var totalammount = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == AttackingEmpireID)
                                            .Where(sc => sc.Item1.HullStrength > 0)
                                            .Select(sc => sc).ToList();

                    if (ammountCommandShips != null && totalammount != null && ammountCommandShips.Count > 0 && totalammount.Count > 0)
                    {
                        if (ammountCommandShips.Count > 0)
                        {
                            commandShipModifierAccuracy = 0.01;
                        }
                        if (ammountCommandShips.Count >= totalammount.Count / 10)
                        {
                            commandShipModifierAccuracy = 0.10;
                        }
                    }



                    // CHECKING EXPERIENCE for accuracy and damage control (retilation loop)
                    // Checking ship experienc ein Retilation loop for the attacking ship (station)
                    string ShipExperience = "Unknown";
                    ShipExperience = AttackingShip.Item1.Source.ExperienceRankString;
                    switch (ShipExperience)
                    {

                        case "Unknown":
                            {
                                sourceAccuracy = 0.70;
                                //targetDamageControl = 0.75;
                                break;
                            }
                        case "Green":
                            {
                                sourceAccuracy = 0.50;
                                //targetDamageControl = 0.45;
                                break;
                            }
                        case "Regular":
                            {
                                sourceAccuracy = 0.60;
                                //targetDamageControl = 0.50;
                                break;
                            }
                        case "Veteran":
                            {
                                sourceAccuracy = 0.70;
                                //targetDamageControl = 0.55;
                                break;
                            }
                        case "Elite":
                            {
                                sourceAccuracy = 1.00;
                                //targetDamageControl = 0.65;
                                break;
                            }
                        case "Legendary":
                            {
                                sourceAccuracy = 1.1;
                                //targetDamageControl = 0.75;
                                break;
                            }
                    }
                    if (AttackingShip.Item1.Name.Contains("!"))
                    {
                        sourceAccuracy = sourceAccuracy + 0.80; // If attacking ship is also a hero ship add 0.8 accuracy.

                    }
                    // All non-command ships get bonus
                    if (AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("CRUISER") || AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("DESTROYER")
                        || AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("FRIGATE") ||
                        (AttackingShip.Item1.Source.Owner.Key.Contains("BORG") &&
                        AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("SPHERE") ||
                        AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("DIAMOND") ||
                        AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("CUBE") 
                        )
                        )
                    {
                        sourceAccuracy = sourceAccuracy + commandShipModifierAccuracy; // taking Command ship present or no present modfier into account.
                    }
                    else if (!AttackingShip.Item1.Source.OrbitalDesign.Key.Contains("COMMAND")) // Civilian Non-Command ship only get negative mod
                    {
                        if (commandShipModifierAccuracy < 0) // only if modifyer is negative
                        {
                            sourceAccuracy = sourceAccuracy + commandShipModifierAccuracy;
                        }
                    }


                    // update x 23 july 2019: added experience to change accuracy and damage control: Checking Target experience in Retilation Loop
                    string TargetShipExperience = "Unknown";
                    TargetShipExperience = currentTarget.Item1.Source.ExperienceRankString;
                    switch (TargetShipExperience)
                    {

                        case "Unknown":
                            {
                                //sourceAccuracy = 0.75;
                                targetDamageControl = 0.55;
                                break;
                            }
                        case "Green":
                            {
                                //sourceAccuracy = 0.45;
                                targetDamageControl = 0.40;
                                break;
                            }
                        case "Regular":
                            {
                                //sourceAccuracy = 0.50;
                                targetDamageControl = 0.45;
                                break;
                            }
                        case "Veteran":
                            {
                                //sourceAccuracy = 0.55;
                                targetDamageControl = 0.50;
                                break;
                            }
                        case "Elite":
                            {
                                //sourceAccuracy = 0.65;
                                targetDamageControl = 0.57;
                                break;
                            }
                        case "Legendary":
                            {
                                //sourceAccuracy = 0.75;
                                targetDamageControl = 0.65;
                                break;
                            }

                    }
                    if (currentTarget.Item1.Source.IsMobile == false)
                        targetDamageControl = targetDamageControl + 0.55;
                    if (currentTarget.Item1.Name.Contains("!"))
                    {
                        targetDamageControl = targetDamageControl + 0.55;
                    }

                    double combatOrderBonusMalus = 0;
                    // Engage rush formation
                    if (attackerOrder == CombatOrder.Engage && (defenderOrder == CombatOrder.Rush || defenderOrder == CombatOrder.Formation))
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.12;
                    // RAID Transports
                    if (attackerOrder == CombatOrder.Transports && defenderOrder != CombatOrder.Formation) // if Raid, and no Formation select Transportships to be targeted
                    {
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.17;
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
                            combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.13; // even more weapon Bonus if defender is Engaging
                    }
                    else if ((attackerOrder == CombatOrder.Transports && defenderOrder == CombatOrder.Formation && _combatStation is null)) // IF Raiding and Defender is doing combat Formating, let Frigates protect Transports
                    {
                        // HEREX
                        currentTargets = _combatShipsTemp.Where(sc => sc.Item1.OwnerID == targetedEmpireID)
                            .Where(sc => sc.Item1.HullStrength > 0)
                            .Where(sc => sc.Item1.Source.OrbitalDesign.Key.Contains("FRIGATE"))
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
                    if(currentTargets.Count == 0)
                    {
                        GameLog.Core.CombatDetails.DebugFormat("Retaliation target not available, but no break and code continues...? to determin bonus... PROBLEM!");
                        // Update attacking Ships weapons not nessecary because no weapons fired
                        //AttackingShip.Item1.RemainingFirepower = remainingFirepowerInWhile;
                    }
                    // Rush
                    if (attackerOrder == CombatOrder.Rush && (defenderOrder == CombatOrder.Retreat || defenderOrder == CombatOrder.Transports))
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.12;
                    // Formation
                    if (attackerOrder == CombatOrder.Formation && (defenderOrder == CombatOrder.Transports || defenderOrder == CombatOrder.Rush))
                        combatOrderBonusMalus = combatOrderBonusMalus + applyDamage * 0.17;

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
                        ScissorBonus = applyDamage * 0.35; // 20 % Scissor Bonus
                    // We have now calculated all bonuses/maluses
                    GameLog.Core.CombatDetails.DebugFormat("added bonuses to retailiation firepower. OrderBonus = {0}, ScissorBonus = {1}", combatOrderBonusMalus, ScissorBonus);
                    // DO I USE remainingFirePowerinWHile OR applyDamage
                    // Do we have more Weapons then target has shields? FirepowerRemains... /// NEW123 added combatOrderBonusMallus and other changes // Maneuverability 8 = 33% more shields. 1 = 4% more shields
                    int check = currentTarget.Item1.Source.GetManeuverablility(); // allows to check if maneuverability is gotten correctly
                                                                                  //if (additionalRun) // If True, it means that we are on the next target
                                                                                  //{
                                                                                  //    if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) >
                                                                                  //    applyDamage) // if remainingFirepower is absorbed by targets Hull/shields/Maneuverability, set it to -1 and discharge weapons.
                                                                                  //    {
                                                                                  //        additionalRun = false; // if target can absorb remaining returnFireFirepower, no more targets nessecary.
                                                                                  //        GameLog.Core.CombatDetails.DebugFormat("it was an additional reteliation run, weapons now fully applied");
                                                                                  //        //applyDamage = remainingFirepowerInWhile;  // save damage to apply damage for dealing damage below

                    //        //{
                    //        //    returnFireFirepower = returnFireFirepower - applyDamage;  // Total Remaining FIrepower for this retaliation
                    //        //    needAdditionalAttackingShip = true; // Note that current Attacker is now empty on firepower, we need more ships to apply returnFireFirepower
                    //        //}
                    //        //else
                    //        //{
                    //        //    needAdditionalAttackingShip = false;
                    //        //}


                    //        //foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire))
                    //        //{
                    //        //        weapon.Discharge();
                    //        //}
                    //        //AttackingShip.Item1.RemainingFirepower = 0; // Set AttackingShips Firepower to 0

                    //            // SETTING SHIP WEAPONS TO 0
                    //        //foreach (var ship in _combatShipsTemp)
                    //        //{
                    //        //    //if(ship.Item1.Source.ObjectID == AttackingShip.Item1.Source.ObjectID)
                    //        //    //    _combatShipsTemp.
                    //        //}
                    //    }
                    //    else
                    //    { // AttackingSHip still has firepower for another run
                    //      // target gets destroied
                    //        additionalRun = true;
                    //        if (returnFireFirepower > 0)
                    //        {
                    //            returnFireFirepower = returnFireFirepower - applyDamage;  // Total Remaining FIrepower for this retaliation
                    //                                                                      //needAdditionalAttackingShip = true; // Note that current Attacker is now empty on firepower, we need more ships to apply returnFireFirepower
                    //            additionalRun = true;
                    //        }
                    //        else
                    //        {
                    //            GameLog.Core.CombatDetails.DebugFormat("Should never be reached, because we have more weapons then target in if. no else possible"); 
                    //        }



                    //        //remainingFirepowerInWhile = applyDamage
                    //        //                    - Convert.ToInt32((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24)) / 100);
                    //        //GameLog.Core.CombatDetails.DebugFormat("it was an addtional run, we still have firepower =  {0}", remainingFirepowerInWhile);
                    //        // Otherwise we still have remainingFirepower
                    //    }
                    //}
                    //if (!additionalRun) // If false, it means that we are on first target
                    //{
                    //    //if ((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * (1 + (currentTarget.Item1.Source.GetManeuverablility() / 0.24) / 100) >
                    //    //    applyDamage * sourceAccuracy + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus)) // First target absorbs weapons of first attacker
                    //    //{
                    //        if (returnFireFirepower > 0)
                    //        {
                    //            returnFireFirepower = returnFireFirepower - applyDamage;
                    //            needAdditionalAttackingShip = true;
                    //        }
                    //        else
                    //        {
                    //            needAdditionalAttackingShip = false;
                    //        }

                    //         // remember damage for apply damage
                    //         // next retaliation ship shell fire with this
                    //        remainingFirepowerInWhile = -1; // End this ships firepower and make additional run with new shiop
                    //        needAdditionalAttackingShip = true;
                    //        foreach (var weapon in AttackingShip.Item2.Where(w => w.CanFire))
                    //        {
                    //            weapon.Discharge();
                    //        }
                    //        AttackingShip.Item1.RemainingFirepower = 0; // Set firepower to 0
                    //        GameLog.Core.CombatDetails.DebugFormat("Retailiation first run ends with first attacking ship has no more weapons. More ships needed");
                    //    }
                    //    else
                    //    {
                    //        // RemainingFirepowerInWhile = remaining firepower of 1 attacking ship
                    //        remainingFirepowerInWhile = Convert.ToInt32(applyDamage * sourceAccuracy) + Convert.ToInt32(ScissorBonus) + Convert.ToInt32(combatOrderBonusMalus)
                    //                            - (currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength);
                    //        GameLog.Core.CombatDetails.DebugFormat("First Retailiation run on target, weapons = {0} reaim", remainingFirepowerInWhile);
                    //        needAdditionalAttackingShip = false;
                    //        // Current Attacking Ship still has weapons remaining.
                    //    }
                    //}
                    /// APPLY DAMAGE IN RETALIATION
                    // Fire Weapons, inflict damage. Either with all calculated bonus/malus. Or if this was done last turn, use remainingFirepower (if any)
                    Random zufall = new Random();

                    // APPLY DAMAGE 

                    double tempDamageInRetaliation = 0;
                   
                    currentTarget.Item1.TakeDamage((int)(Convert.ToInt32(tempDamageInRetaliation =
                        Convert.ToDouble((applyDamage + (ScissorBonus) + (combatOrderBonusMalus))
                        * Convert.ToDouble(1- currentTarget.Item1.Source.GetManeuverablility() /0.24 / 100))
                            * Convert.ToDouble(1.5 - targetDamageControl) * 
                            (zufall.Next(8, 13)/10) * sourceAccuracy ))); // minimal damage of 50 included


                    GameLog.Core.CombatDetails.DebugFormat("target ship in retaliation: {0} suffered damage: {1} +bonus-randum. it has hull left: {2}", currentTarget.Item1.Name, tempDamageInRetaliation, currentTarget.Item1.HullStrength);
                    //GameLog.Core.CombatDetails.DebugFormat("Retailiation damage of this round, now has been applied additional run: {0}, OR first run: {1} + Bonuse", remainingFirepowerInWhile, applyDamage);



                    // Knows we need more targets
                    if (additionalRun) // If additional run = true it means the attacking ship has more weapons then targetsShields/Hull
                    {
                        applyDamage = applyDamage - Convert.ToInt32((currentTarget.Item1.ShieldStrength + currentTarget.Item1.HullStrength) * 
                            (1+ currentTarget.Item1.Source.GetManeuverablility() / 0.24 /100));
                        if (applyDamage <= 0) // Make sure remaining weapons are not increased by minus-damage
                            applyDamage = 0;
                    }// damage has beeen applied, apply Damage set to remaining Weapons




                    // More Attacker, yes or no?
                    if (returnFireFirepower > 0)
                    {
                            needAdditionalAttackingShip = true; // We still need to apply retaliation, we need more ships
                        GameLog.Core.CombatDetails.DebugFormat("Retailiation incomplete. Have ReturnFireFirepower. Need more attacking ships to apply it");
                    }
                    else
                        {
                            needAdditionalAttackingShip = false; // We have fully applied returnFireFirepower, no more attacker
                        if (applyDamage <= 0)
                        {

                            additionalRun = false;
                        }
                            ///returnFireFirepower = 0 does not mean we do not need more targets. It just says current attacker has enought firepower left (apply damage)
                            GameLog.Core.CombatDetails.DebugFormat("Retailiation complete. No more Attacker nessarcy. Update AttackingShips weapons. Break");
                            AttackingShip.Item1.RemainingFirepower = AttackingShip.Item1.RemainingFirepower - applyDamage; // Weapons remain
                            
                        }

                    
                    
                    
                }
                #endregion
                // End Return fire

                #region end of combat now  house keeping
                GameLog.Core.CombatDetails.DebugFormat("CHECK IF ANOTHER TOTAL LOOP: IndexofAttackerEmpire = {0}", indexOfAttackerEmpires);
                indexOfAttackerEmpires = indexOfAttackerEmpires + 1; // The next Empire in the Array gets its shot in the next whileloop
                GameLog.Core.CombatDetails.DebugFormat("IndexOfAttackerEmpire now = {0}", indexOfAttackerEmpires);
                // SWITCH TO NEXT EMPIREs ACTIVE FIRING OR END THE BATTLE
                if (indexOfAttackerEmpires > 11 || empiresInBattle[indexOfAttackerEmpires, 0] == 999)
                {
                    indexOfAttackerEmpires = 0;
                    while (empiresInBattle[indexOfAttackerEmpires, 0] == 999)
                    {
                        indexOfAttackerEmpires += 1;
                        if (empiresInBattle[indexOfAttackerEmpires, 0] != 999)
                        { 
                        GameLog.Core.CombatDetails.DebugFormat("ANOTHER TOTAL LOOP IndexOfAttackerEmpire now = {0}", indexOfAttackerEmpires);
                        break;
                        }
                        if (indexOfAttackerEmpires > 11)
                        {
                            activeBattle = false;
                            GameLog.Core.CombatDetails.DebugFormat("NO MORE TOTAL LOOP IndexOfAttackerEmpire now = {0}", indexOfAttackerEmpires);
                            break;
                        }
                        TargetOneORTwo = TargetOneORTwo + 1; // cycle to next targeted empire
                        GameLog.Core.CombatDetails.DebugFormat("ANOTHER TOTAL LOOP, with Target {0}",TargetOneORTwo);
                    }
                }

                // Once all empires have fired once, the first empire fires again
                GameLog.Core.CombatDetails.DebugFormat("Current Empire about to fire: {0}", empiresInBattle[indexOfAttackerEmpires, 0]);

                // This is the closing of the Entire battle loop

            }
            // NEXT EMPIRE
            // Once no more ships available, close loop
            // Update _combatShips to current _combatShipsTemp
            // Investigate how and where "friendlyships" etc. are used to display remaining ships fitting to the screen
            // FINISH BATTLE destroy ships/stations
            GameLog.Core.CombatDetails.DebugFormat("THE ENTIRE BATTLE WAS FULLY COMPLETED. May need to remove destroyed ships");
            
            // IN here is my code (while x3)

            //for (int i = 0; i < _combatShips.Count; i++)
            //{
            //    GameLog.Core.CombatDetails.DebugFormat("the _combatShip[i] ={0}", _combatShips[i].Item1.Name);
            //    GameLog.Core.CombatDetails.DebugFormat("_combatShipTemp[i] ={0}", _combatShipsTemp[i].Item1.Name);
            //}

            // break out of while loop end combat
            //End of Combat:
          
            int countDestroyed = 0;
            foreach (var combatent in _combatShipsTemp) // now search for destroyed ships
            {
                GameLog.Core.CombatDetails.DebugFormat("IsDestroid = {2} (Hull = {3}) for combatent {0} {1} if true see second line ", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.IsDestroyed, combatent.Item1.HullStrength);
                if (combatent.Item1.IsDestroyed)
                {
                    //GameLog.Core.CombatDetails.DebugFormat("Combatent {0} {1} IsDestroid ={2} if true see second line Hull ={3}", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.IsDestroyed, combatent.Item1.HullStrength);
                    var Assets = GetAssets(combatent.Item1.Owner);
                    Assets.AssimilatedShips.Remove(combatent.Item1);
                    GameLog.Core.Combat.DebugFormat("Combatent {0} {1} ({2}) was destroyed", combatent.Item1.Source.ObjectID, combatent.Item1.Name, combatent.Item1.Source.Design);
                    if (combatent.Item1.Source is Ship)
                    {
                        if (Assets != null)
                        {
                            GameLog.Core.CombatDetails.DebugFormat("REMOVE DESTROYED Name of Owner = {0}, Assets.CombatShips = {1}, Assets.NonCobatShips = {2}", Assets.Owner.Name, Assets.CombatShips.Count, Assets.NonCombatShips.Count);
                            if (!Assets.DestroyedShips.Contains(combatent.Item1))
                            {
                                Assets.DestroyedShips.Add(combatent.Item1);
                            }
                            if (combatent.Item1.Source.IsCombatant)
                            {
                                countDestroyed += 1;

                                Assets.CombatShips.Remove(combatent.Item1);
                            }
                            else
                            {
                                Assets.NonCombatShips.Remove(combatent.Item1);
                            }
                        }
                        else
                            GameLog.Core.CombatDetails.DebugFormat("Assets Null");
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
            //if (true) // End Combat after 3 While loops
            //{
                GameLog.Core.CombatDetails.DebugFormat("NOW FORCE ALL TO RETREAT THAT WHERE NOT DESTROYED, Number of destroyed ships in total: {0}", countDestroyed);
                //GameLog.Core.CombatDetails.DebugFormat("round# ={0}", _roundNumber);
                //_roundNumber += 1;
                //GameLog.Core.CombatDetails.DebugFormat("round# ={0} now", _roundNumber);
                // _combatShips = _combatShipsTemp;
           
                var allRetreatShips = _combatShipsTemp // All non destroyed ships retreat (survive)
                    .Where(s => !s.Item1.IsDestroyed)
                    .Where(s => s.Item1.Owner != s.Item1.Source.Sector.Owner) // Ships in own territory make a stand (remain in the system they own), after 5 turns.
                    .ToList();
                foreach (var ship in allRetreatShips)
                {
                    if (ship.Item1 != null)
                    {
                        GameLog.Core.CombatDetails.DebugFormat("END retreated ship = {0} {1}", ship.Item1.Name, ship.Item1.Description);
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        if (!ownerAssets.EscapedShips.Contains(ship.Item1))
                        {
                            GameLog.Core.CombatDetails.DebugFormat("END EscapedShips ={0}", ship.Item1.Name);
                            ownerAssets.EscapedShips.Add(ship.Item1);
                            ownerAssets.CombatShips.Remove(ship.Item1);
                            ownerAssets.NonCombatShips.Remove(ship.Item1);
                            _combatShips.Remove(ship);
                        }
                    }
                }
            //}
            //********************************************************************

            GameLog.Core.CombatDetails.DebugFormat("AutomatedCombatEngine ends");

        }// END OF RESOVLECOMBATROUNDCORE
    }
}
#endregion
