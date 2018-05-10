// AutomatedCombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System.Collections.Generic;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Types;
using Supremacy.Utility;

using Wintellect.PowerCollections;

namespace Supremacy.Combat
{
    public sealed class AutomatedCombatEngine : CombatEngine
    {
        private const double BaseChanceToRetreat = 0.25;
        private const double BaseChangeAssimilation = 1;
        private readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        private readonly List<Pair<CombatUnit, CombatWeapon[]>> _combatShips;
        private Pair<CombatUnit, CombatWeapon[]> _combatStation;

        //private bool _automatedCombatTracing = true;    // turn to true if you want gamelogs
        private bool _automatedCombatTracing = false;    // turn to true if you want
        //private bool _iautomatedCombatTracing = true;
        private bool _iautomatedCombatTracing = false;
        private Civilization sourceOwner;

        public AutomatedCombatEngine(
            IList<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {
            // TODO: This looks like a waste of time, table is not going to change from one combat to the other
            // Consider creating the table once instead
            var accuracyTable = GameContext.Current.Tables.ShipTables["AccuracyModifiers"];
            _experienceAccuracy = new Dictionary<ExperienceRank, double>();
            foreach (ExperienceRank rank in EnumUtilities.GetValues<ExperienceRank>())
            {
                _experienceAccuracy[rank] = Number.ParseDouble(accuracyTable[rank.ToString()][0]);
            }
            ///////////////////

            _combatShips = new List<Pair<CombatUnit, CombatWeapon[]>>();

            foreach (CombatAssets civAssets in Assets)
            {
                if (civAssets.Station != null)
                {
                    _combatStation = new Pair<CombatUnit, CombatWeapon[]>(
                        civAssets.Station,
                        CombatWeapon.CreateWeapons(civAssets.Station.Source));
                }
                foreach (CombatUnit shipStats in civAssets.CombatShips)
                {
                    _combatShips.Add(new Pair<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));
                }
                foreach (CombatUnit shipStats in civAssets.NonCombatShips)
                {
                    _combatShips.Add(new Pair<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));
                }
            }
        }

        #region ***ResolveCombateRoundCore
        //int iCount = 0;

        //public static bool IsAssimilated { get; internal set; }

        protected override void ResolveCombatRoundCore()
        {
            bool combatOccurred;

            if (_combatStation.First != null)
            {
                //GameLog.Print("_combatStation.Count={0}", _combatStation.Count);
                for (int i = 0; i < _combatStation.Second.Length; i++)
                    _combatStation.Second[i].Recharge();
            }

            if (_automatedCombatTracing)
                GameLog.Print("_combatShips.Count={0}", _combatShips.Count);

            for (int i = 0; i < _combatShips.Count; i++)
            {
                for (int j = 0; j < _combatShips[i].Second.Length; j++)
                    _combatShips[i].Second[j].Recharge();
            }
            #region ** do while 'combatOccured' loop in ResolveCombatRoundCore
            do // run this code at least once then check at the 'while' for combatOccured = true
            {
                combatOccurred = false;

                Algorithms.RandomShuffleInPlace(_combatShips);

                for (int i = 0; i < _combatShips.Count; i++)
                {
                    if (_combatShips.Count == i)
                    {
                        break; 
                    }
                    CombatAssets ownerAssets = GetAssets(_combatShips[i].First.Owner);
                    CombatUnit target;
                    //if (_combatShips[i].First.Owner.Name == "Borg") //attacher is Borg
                    //{

                    //    for (int a = 0; a < _combatShips.Count; a++)
                    //    {
                    //        CombatAssets targetAssets = GetAssets(_combatShips[a].First.Owner);
                    //        CombatAssets sourceAssets = GetAssets(_combatShips[i].First.Owner);
                    //        CombatUnit result = _combatShips[a].First;

                    //        if (_combatShips[a].First.Source is Ship && _combatShips[a].First.Source.IsCombatant == true && _combatShips[a].First.ShieldIntegrity <= 100)
                    //        {
                    //            target = result; // target the ship [a] we just found without shields with the borg ship [i] we found

                    //            int chanceToAssimilate = Statistics.Random(10000) % 100;
                    //            if (chanceToAssimilate <= (int)(BaseChangeAssimilation * 100))
                    //            {

                    //                target.Source.Owner = _combatShips[i].First.Owner;
                    //                target.Source.OwnerID = _combatShips[i].First.OwnerID;
                    //                //assets.AssimilatedShips.Add(assets.CombatShips[i]);
                    //                //assets.CombatShips.RemoveAt(i--);
                                 
                    //                targetAssets.AssimilatedShips.Add(targetAssets.CombatShips[a]);
                    //                targetAssets.CombatShips.RemoveAt(a--);
                    //                //sourceAssets.CombatShips.Add(targetAssets.CombatShips[a]);

                    //                targetAssets.UpdateAllSources();
                    //                combatOccurred = true;
                    //                //GameLog.Print("targetAssets.CombatShips[a] null? {0}", targetAssets.CombatShips[a]);
                    //                break; // one target ship
                    //            }
                    //            //for (int b = 0; b < _combatShips.Count; b++)
                    //            //{
                    //            //    if (_combatShips[b].First.Source == target.Source)
                    //            //    {
                    //            //        _combatShips.RemoveAt(b);
                    //            //        break;
                    //            //    }
                    //            //}
                    //            if (target == null)
                    //            {
                    //                continue;
                    //            }
                    //        }

                    //    }
                    }

                    CombatOrder order = GetOrder(_combatShips[i].First.Source);
                    if (order != CombatOrder.Engage ||
                        order != CombatOrder.Formation ||
                        order != CombatOrder.Hail ||
                        order != CombatOrder.Retreat ||
                        order != CombatOrder.Rush ||
                        order != CombatOrder.Transports)
                        {
                            order = CombatOrder.Engage; // if there is no order make it engage
                        }


                    if (_iautomatedCombatTracing)
                        GameLog.Print("CombatOccured List<Pair<CombatUnit, CombatWeapon[]>> _combateship i = {0}", i);


                    if (order == CombatOrder.Hail)
                    {
                        continue;
                    }

                    else if (order == CombatOrder.Transports) // found ship with order to attack transports
                    {
                        if (_combatShips[i].First.Source.OrbitalDesign.ShipType.ToString() == "Spy" || _combatShips[i].First.Source.OrbitalDesign.ShipType.ToString() == "Construction") // skip over spy ships
                        {
                            continue;
                        }
                        //iCount = _combatShips[i].First.GetHashCode();
                        //GameLog.Print("combatShip i value of iCount = {0}", iCount);
                        //if (_automatedCombatTracing)
                        //    GameLog.Print("Transport-Button: target = {0} {1}", target.Source.ObjectID, target.Source.Name);
                        for (int j = 0; j < _combatShips.Count; j++) // look through all the other ships j
                        {
                            
                            CombatUnit result = _combatShips[j].First; // identify the selected ship for targeting, bypass the ChooseTarget() here
                            CombatOrder otherOrder = GetOrder(_combatShips[j].First.Source); // get the orders for the targeted ship j
                            if (_iautomatedCombatTracing)
                                GameLog.Print("Raid Transports, find Target _combateShip j = {0} for Attacker _combatShip i {1}", j, i);

                            if (_combatShips[j].First.Source.IsCombatant == true && _combatShips[j].First.Source.OrbitalDesign.ShipType.ToString() == "Transport") // find a combatant transport ship j
                            {
                                GameLog.Print("Target ShipType = {0}, IsCombatant = {1}, Attach Ship CombatOrder = {2}, _combatShip[i] = {3}", _combatShips[j].First.Source.OrbitalDesign.ShipType.ToString(), _combatShips[j].First.Source.IsCombatant, order, _combatShips[i].First.Source);
                                target = result; // target the transport combatant ship j for our ship i with raid transport order?
                                if (target != null && otherOrder != CombatOrder.Formation)
                                {
                                    try
                                    {
                                        foreach (CombatWeapon weapon in _combatShips[i].Second) // for each weapon ship i has 
                                        {
                                            if (weapon.CanFire) // if it can fire at a target
                                            {
                                                //order = CombatOrder.Engage;
                                                Attack(_combatShips[i].First, target, weapon, order); // our ship i exicutes the Attach function on it's target with xyz result for ship j target
                                                combatOccurred = true;

                                                if (_automatedCombatTracing)
                                                    GameLog.Print("ChooseTarget - Transport  {0} {1} {2} = {2}", result.OwnerID, result.Name, result.Source.OrbitalDesign.ShipType.ToString());
                                                if (_iautomatedCombatTracing)
                                                    GameLog.Print("Attack Tranport _combateship j = {0} with Attacker _combatShip i = {1}", j, i);
                                                /* break;*/ // done with this pair of weapons from the foreach
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        GameLog.Print("Order Transport try catch");
                                        continue;
                                    }
                                }
                                else if (otherOrder == CombatOrder.Formation)
                                {
                                    target = ChooseTarget(_combatShips[i].First.Owner); // If the ships J Transport, to target, is in Formation then use the ChooseTarget() to find a random target for ship i
                                    foreach (CombatWeapon weapon in _combatShips[i].Second) // for each weapon ship i has 
                                    {
                                        if (target != null)
                                        {
                                            try
                                            {
                                                if (weapon.CanFire) // if it can fire at a target
                                                {
                                                    //order = CombatOrder.Engage;
                                                    Attack(_combatShips[i].First, target, weapon, order); // our ship i exicutes the Attach function on it's target with xyz result for ship j target
                                                    combatOccurred = true;

                                                    if (_automatedCombatTracing)
                                                        GameLog.Print("ChooseTarget - Transport  {0} {1} {2} = {2}", result.OwnerID, result.Name, result.Source.OrbitalDesign.ShipType.ToString());
                                                    if (_iautomatedCombatTracing)
                                                        GameLog.Print("Attack Tranport _combateship j = {0} with Attacker _combatShip i = {1}", j, i);
                                                    /* break;*/ // done with this pair of weapons from the foreach
                                                }
                                            }
                                            catch
                                            {
                                                GameLog.Print("Order Formation try catch");
                                                continue;
                                            }
                                        }
                                    }
                                }
                                continue;
                            }
                            else if (_combatShips[j].First.Source.IsCombatant == true) // if no transports combatent is found conintue to look for a target
                            {
                                target = result;// target the non transport ship j instead with our ship i
                                if (target != null)
                                {
                                    try
                                    {
                                        foreach (CombatWeapon weapon in _combatShips[i].Second) // for each weapon ship i has 
                                        {
                                            if (weapon.CanFire) // if it can fire
                                            {
                                                //order = CombatOrder.Engage;                                       
                                                Attack(_combatShips[i].First, target, weapon, order);
                                                combatOccurred = true;

                                                if (_automatedCombatTracing)
                                                    GameLog.Print("ChooseTarget - Transport  {0} {1} {2} = {2}", result.OwnerID, result.Name, result.Source.OrbitalDesign.ShipType.ToString());
                                                if (_iautomatedCombatTracing)
                                                    GameLog.Print("Attack NonTransport _combateship j = {0} with Attacker _combatShip i = {1} inside Raid Transports", j, i);
                                                /*break;*/ // done with this pair of weapons from the foreach
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        GameLog.Print("No transport found so try looking for other targets; try catch");
                                        continue;

                                    }
                                }
                            }                          
                        }
                        continue; // move on to the next ship i with raid transports order to find other target
                    }
                    else if (order == CombatOrder.Retreat) // found a ship i trying to retreat
                    {
                        for (int k = 0; k < _combatShips.Count; k++) // look at all the other ships k
                        {
                            if (_combatShips[k].First.Source.IsCombatant == true) // true = found a ship k that is a combatant for retreater ship i, I hope
                            {
                                CombatOrder otherOrder = GetOrder(_combatShips[k].First.Source);
                                if (otherOrder == CombatOrder.Rush) // attacking ship k has a rush order
                                {
                                    // ToDo: if fast attack ship odds of retreat reduced more than if no fast attack ships
                                    if (_iautomatedCombatTracing)
                                        GameLog.Print("Rush _combateship k = {0}", k);
                                    int chanceToRetreat = Statistics.Random(10000) % 100;
                                    if (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 10)) // reduce base change to retreat and test for true
                                    {
                                        ownerAssets.EscapedShips.Add(_combatShips[i].First); // add ship i, tyring to retreat, to escaped list
                                        if (_combatShips[i].First.Source.IsCombatant) // true = the ship i, trying to retreat, was a combatant
                                        {
                                            ownerAssets.CombatShips.Remove(_combatShips[i].First); // remove ship i, tyring to retreat, from list CombatShips
                                        }
                                        else
                                        {
                                            ownerAssets.NonCombatShips.Remove(_combatShips[i].First); // remove ship i, tyring to retreat, from list CombatShips
                                        }
                                        if (_iautomatedCombatTracing)
                                            GameLog.Print("Remove Rushed Retreating _combateship i = {0} with Attacker _combatShip k = {1} inside Retreat", i, k);
                                        _combatShips.RemoveAt(i); // take ship off the _combatships list at the prior index
                                    }
                                    if (_iautomatedCombatTracing)
                                        GameLog.Print("After RemoveAt(i) index Rushed Retreating _combateship i = {0} with Attacker _combatShip k = {1} inside Retreat", i, k);
                                }

                                else // if none of the attacking ships have the rush order you are left with Base Change to Retreat
                                {
                                    int chanceToRetreat = Statistics.Random(10000) % 100;
                                    if (chanceToRetreat <= (int)(BaseChanceToRetreat * 100))
                                    {
                                        ownerAssets.EscapedShips.Add(_combatShips[i].First);
                                        if (_combatShips[i].First.Source.IsCombatant)
                                        {
                                            ownerAssets.CombatShips.Remove(_combatShips[i].First);
                                        }
                                        else
                                        {
                                            ownerAssets.NonCombatShips.Remove(_combatShips[i].First);
                                        }
                                        if (_iautomatedCombatTracing)
                                            GameLog.Print("Remove not Rushed Retreating _combateship i = {0} with Attacker _combatShip k = {1} inside Retreat", i, k);
                                        _combatShips.RemoveAt(i);
                                    }

                                }
                            }
                            continue; 
                        }
                        if (_iautomatedCombatTracing)
                            GameLog.Print("End of Retreating _combateship i = {0}", i);
                        continue; 
                    }                
                    else // if (i < _combatShips.Count && i > 0)// Is there still a ship i not retreating and not raiding transports? if so go find a target
                    {
                        target = ChooseTarget(_combatShips[i].First.Owner); // use ChooseTarget to find something to shoot at
                        //if (target == null)
                        //{
                        //    continue;
                        //}

                        //if (_automatedCombatTracing)
                        //        GameLog.Print("target = {0} {1}", target.Source.ObjectID, target.Source.Name);
                        if (target != null)
                        {
                            try
                            {
                                foreach (CombatWeapon weapon in _combatShips[i].Second)
                                {
                                    if (weapon.CanFire)
                                    {
                                        int targetIndex = -1;
                                        for (int m = 0; m < _combatShips.Count; m++)
                                        {
                                            if (_combatShips[m].First.Source == target.Source) // if ship i has weapon that can fire and we find a target for the weapon set targetIndex here and break
                                            {
                                                targetIndex = m;
                                                if (_iautomatedCombatTracing)
                                                    GameLog.Print("targetIndex m = {0} _combatShip i = {1} near end of while combatOccured", m, i);
                                                break;
                                            }
                                        }
                                        combatOccurred = true; // found the target above so now combat occurred
                                        if (Attack(_combatShips[i].First, target, weapon, order))  // execute Attack() methode for ship i target ship m
                                        {
                                            if (targetIndex < i) // if the targetIndex for weapon is prior to current ship i being targeted then move backup on i by subtract one and break? Do not increment onto next ship but stay here and do while another loop for more weapons?
                                                --i; // 
                                            if (_iautomatedCombatTracing)
                                                GameLog.Print("_combatShip i = {1} at end of while combatOccured", i);

                                        }

                                        break;
                                    }
                                }
                            }
                            catch
                            {
                                GameLog.Print("try catch");
                                continue;

                            }
                        }
                    }
                }

                

                if ((_combatStation.First != null) && !_combatStation.First.IsDestroyed) // there is a station
                {
                    CombatUnit target = ChooseTarget(_combatStation.First.Owner);  // use ChooseTarget to find target for station? 

                    if (_automatedCombatTracing)
                        GameLog.Print("target is a station = {0} {1}", target.Source.ObjectID, target.Source.Name);

                    if (target != null)
                    {
                        try
                        {
                            foreach (CombatWeapon weapon in _combatStation.Second) // each station weapon
                            {
                                if (weapon.CanFire)
                                {
                                    CombatOrder order = default(CombatOrder);
                                    Attack(_combatStation.First, target, weapon, order);
                                    combatOccurred = true;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                            GameLog.Print("try catch");
                            continue;

                        }
                    }
                }
            } while (combatOccurred); // continue code past here when combat is over but otherwise back to do in this loop
            #endregion ** End - combatOccured loop

            for (int i = 0; i < _combatShips.Count; i++)
            {
                //CombatAssets ownerBorg = GetAssets(_combatShips[i].First.Owner);
                //if (ownerBorg.Owner.Name.ToString() != "Borg") continue;

                //CombatOrder order = GetOrder(_combatShips[i].First.Source);

                if (_combatShips[i].First.IsCloaked)
                {
                    _combatShips[i].First.Decloak();
                }

            }

            if (_automatedCombatTracing)
                GameLog.Print("ResolveCombatRoundCore is done...");
        }
        #endregion *** End - ResolveCombat...

        #region ChooseTarget
        private CombatUnit ChooseTarget(Civilization sourceOwner)
        {
            CombatUnit result = null;
            int start = Statistics.Random(_combatShips.Count);

            for (int i = start; i < _combatShips.Count; i++)
            {
                if (CombatHelper.WillEngage(_combatShips[i].First.Owner, sourceOwner))  // friends or foe ?, true = found a foe ship
                {
                    if (!_combatShips[i].First.IsCloaked || (RoundNumber > 1))   // true = uncloaked ship pasted round one, THIS is the PRELIMINARY target.
                    {
                        result = _combatShips[i].First; // target ship
                        //if (_automatedCombatTracing)
                        //    GameLog.Print("ChooseTarget is preliminary {0} {1}", result.OwnerID, result.Name);
                        break;   // found one target for the combatent ship using ChooseTarget()
                    }
                }
            }

            if (result == null)   // if result is still null -> not null if cloaked and round 2 or more
            {
                for (int i = 0; i < start; i++) // pick up the search where we left off
                {
                    if (CombatHelper.WillEngage(_combatShips[i].First.Owner, sourceOwner))   // friends or foe ?, true = found a foe ship
                    {
                        if (!_combatShips[i].First.IsCloaked || (RoundNumber > 1)) // true = uncloaked ship pasted round one, THIS is the PRELIMINARY target.
                        {
                            result = _combatShips[i].First; // target ship
                            if (_automatedCombatTracing)
                                GameLog.Print("ChooseTarget is {0} {1} after result one was empty", result.OwnerID, result.Name);
                            break;
                        }
                    }
                }
            }

            if ((_combatStation.First != null)   // always calculated
                && !_combatStation.First.IsDestroyed
                && (sourceOwner != _combatStation.First.Owner)
                && ((result == null) || (Statistics.Random(4) == 0)))    // if no ship to target OR everytime: Random about 25% of weapons fired to the station
            {
                result = _combatStation.First;
                if (_automatedCombatTracing)
                    GameLog.Print("ChooseTarget is a station {0} {1}", result.OwnerID, result.Name);
            }

            return result;
        }
        #endregion End - ChooseTarget

        private bool Attack(CombatUnit source, CombatUnit target, CombatWeapon weapon, CombatOrder order) // bool Attack
        {

            int accuracy = (int)(_experienceAccuracy[source.Source.ExperienceRank] * 100);

            if (Statistics.Random(100) >= (100 - accuracy))
            {
                target.TakeDamage(weapon.MaxDamage.CurrentValue);
            }
            weapon.Discharge();

            if (target.IsDestroyed)
            {
                CombatAssets targetAssets = GetAssets(target.Owner);
                if (target.Source is Ship)
                {
                    if (target.Source.IsCombatant)
                    {
                        for (int i = 0; i < targetAssets.CombatShips.Count; i++)
                        {
                            if (targetAssets.CombatShips[i].Source == target.Source)
                            {
                                targetAssets.DestroyedShips.Add(targetAssets.CombatShips[i]);
                                targetAssets.CombatShips.RemoveAt(i);
                                GameLog.Print("Combatships[i] at target.Isdestroyed, i = {0}", i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < targetAssets.NonCombatShips.Count; i++)
                        {
                            if (targetAssets.NonCombatShips[i].Source == target.Source)
                            {
                                targetAssets.DestroyedShips.Add(targetAssets.NonCombatShips[i]);
                                targetAssets.NonCombatShips.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < _combatShips.Count; i++)
                    {
                        if (_combatShips[i].First.Source == target.Source)
                        {
                            _combatShips.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            return target.IsDestroyed;
        }

        //private bool TryAssimilation(CombatUnit source, CombatUnit target) // bool 
        //{

        //    if (_automatedCombatTracing)
        //        GameLog.Print("{0} = target.IsAssmilated, {1} = source.Owner, {2} = target.Owner.Name", target.IsAssimilated, source.Owner.ToString(), target.Owner.Name.ToString());

        //    CombatAssets targetAssets = GetAssets(target.Owner);
        //    CombatAssets sourceAssets = GetAssets(source.Owner);
        //    if (target.Source is Ship)
        //    {
        //        //target.Source.Scrap = true;
        //        if (target.Source.IsCombatant)
        //        {
        //            for (int i = 0; i < targetAssets.CombatShips.Count; i++)
        //            {
        //                if (targetAssets.CombatShips[i].Source == target.Source && target.ShieldIntegrity == 0)
        //                {

        //                    target.Source.Owner = source.Owner;
        //                    target.Source.OwnerID = source.OwnerID;
        //                    targetAssets.AssimilatedShips.Add(targetAssets.CombatShips[i]);
        //                    sourceAssets.CombatShips.Add(targetAssets.CombatShips[i]);
        //                    targetAssets.CombatShips.RemoveAt(i);
        //                    targetAssets.UpdateAllSources();
        //                    break;
        //                }
                        
        //            }
        //        }

        //    }
        //    else
        //    {
        //        for (int i = 0; i < targetAssets.NonCombatShips.Count; i++)
        //        {
        //            if (targetAssets.NonCombatShips[i].Source == target.Source && target.ShieldIntegrity == 0)
        //            {
        //                target.Source.Owner = source.Owner;
        //                target.Source.OwnerID = source.OwnerID;
        //                targetAssets.AssimilatedShips.Add(targetAssets.NonCombatShips[i]);
        //                sourceAssets.CombatShips.Add(targetAssets.CombatShips[i]);
        //                targetAssets.NonCombatShips.RemoveAt(i);
        //                targetAssets.UpdateAllSources();
        //                break;
        //            }
                    
        //        }
        //    }
        //    for (int i = 0; i < _combatShips.Count; i++)
        //    {
        //        if (_combatShips[i].First.Source == target.Source)
        //        {
        //            _combatShips.RemoveAt(i);
        //            break;
        //        }
        //    }
        
        //}
        
    }
}