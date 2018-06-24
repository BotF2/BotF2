// AutomatedCombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Combat
{
    public sealed class AutomatedCombatEngine : CombatEngine
    {
        public AutomatedCombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {

        }

        protected override void ResolveCombatRoundCore()
        {
            //Recharge all of the weapons
            if (_combatStation != null)
            {
                foreach (var weapon in _combatStation.Item2)
                {
                    weapon.Recharge();
                }
            }
            foreach (var combatShip in _combatShips)
            {
                foreach (var weapon in combatShip.Item2)
                {
                    weapon.Recharge();
                }

            }


           // GameLog.Print("Owner Group 1{0}, 2{1}, 3{2}", OwnerGroup1, OwnerGroup2, OwnerGroup3);
            _combatShips.ShuffleInPlace();
            int weaponPowerFriendly = 0;
            int weaponPowerHostile = 0;

            for (int i = 0; i < _combatShips.Count; i++)
            {

                var ownerAssets = GetAssets(_combatShips[i].Item1.Owner);
                var oppositionShips = _combatShips.Where(cs => CombatHelper.WillEngage(_combatShips[i].Item1.Owner, cs.Item1.Owner));
                var friendlyShips = _combatShips.Where(cs => !CombatHelper.WillEngage(_combatShips[i].Item1.Owner, cs.Item1.Owner));
                GameLog.Print("ownerAssets.Owner {0}", ownerAssets.Owner);

                foreach (var oppositionShip in oppositionShips)
                {
                    foreach (var weapon in oppositionShip.Item2.Where(w => w.CanFire))
                    {

                        {
                            weaponPowerHostile = weaponPowerHostile + CombatHelper.CalculateOrbitalPower(oppositionShip.Item1.Source);
                            //if (_traceCombatEngine)
                            //{
                            //    GameLog.Print("{0} {1}: weaponPowerHostile: {2}", _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponPowerHostile);
                            //}
                        }
                        if (_traceCombatEngine)
                        {
                            GameLog.Print("{0} {1}: weaponPowerHostile: {2}", _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponPowerHostile);
                        }
                    }
                }
                foreach (var friendlyShip in friendlyShips)

                {

                    foreach (var weapon in friendlyShip.Item2.Where(w => w.CanFire))
                    {
                        weaponPowerFriendly = weaponPowerFriendly + CombatHelper.CalculateOrbitalPower(friendlyShip.Item1.Source);
                        //if (_traceCombatEngine)
                        //{
                        //    GameLog.Print("{0} {1}: weaponPowerFriendly: {2}", _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponPowerFriendly);
                        //}
                    }
                    if (_traceCombatEngine)
                    {
                        GameLog.Print("{0} {1}: weaponPowerFriendly: {2}", _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponPowerFriendly);
                    }
                }

                bool oppositionIsRushing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Rush));
                bool oppositionIsInFormation = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Formation));
                var order = GetOrder(_combatShips[i].Item1.Source);

                switch (order)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Rush:
                    case CombatOrder.Transports:
                    case CombatOrder.Formation:

                        var attackingShip = _combatShips[i].Item1;
                        var target = ChooseTarget(_combatShips[i].Item1);
                        int chanceRushingFormation = Statistics.Random(10000) % 100;
                        bool takeDamageRushingFormation;

                        if (target == null && _traceCombatEngine)
                        {
                            GameLog.Print("No target for {1} {0}", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID);
                        }
                        if (target != null)
                        {
                            if (_traceCombatEngine)
                            {
                                GameLog.Print("Target for {1} {0} is {2} {3}", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID, target.Source.ObjectID, target.Name);
                            }

                            // if we rushed a formation we could take damaga
                            if ((oppositionIsInFormation) && (order == CombatOrder.Rush))
                            {
                                var hullIntegrity = attackingShip.HullIntegrity;
                                takeDamageRushingFormation = (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100)));
                                if (takeDamageRushingFormation)
                                {
                                    GameLog.Print("Rushing ship {0} ? take damage{1}", _combatShips[i].Item1.Name, takeDamageRushingFormation);
                                    hullIntegrity = hullIntegrity / 2;
                                }

                            }
                            bool assimilationSuccessful = false;
                            //If the attacker is Borg, try and assimilate before you try destroying it
                            if (attackingShip.Owner.Name == "Borg" && target.Owner.Name != "Borg")
                            {
                                if (_traceCombatEngine)
                                {
                                    GameLog.Print("{0} attempting assimilation on {1} ", _combatShips[i].Item1.Name, target.Owner.Name);
                                }
                                int chanceToAssimilate = Statistics.Random(10000) % 100;
                                assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                            }

                            //Perform the assimilation, but only on ships
                            if (assimilationSuccessful && target.Source is Ship)
                            {
                                if (_traceCombatEngine)
                                {
                                    GameLog.Print("{0} {1} successfully assimilated {2} {3}", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID, target.Name, target.Source.ObjectID);
                                }

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

                            }

                            //If we're not assimilating, destroy it instead :)
                            else
                            {
                                if (_traceCombatEngine)
                                {
                                    GameLog.Print("{0} {1} attacking {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);
                                }

                                foreach (var weapon in _combatShips[i].Item2.Where(w => w.CanFire))
                                {

                                    if (!target.IsDestroyed)
                                    {
                                        PerformAttack(attackingShip, target, weapon);
                                    }
                                }
                            }
                        }

                        foreach (var combatShip in _combatShips)
                        {
                            if (combatShip.Item1.IsDestroyed)
                                ownerAssets.AssimilatedShips.Remove(target);
                        }

                        break;

                    case CombatOrder.Retreat:
                        //Calculate the the odds
                        int chanceToRetreat = Statistics.Random(10000) % 100;
                        bool retreatSuccessful;
                        if (weaponPowerFriendly == 0)
                        {
                            weaponPowerFriendly = 1;
                        }
                        // weaponPowerHostile = 99;
                        decimal weaponRatio = 9;
                        weaponRatio = weaponPowerHostile / weaponPowerFriendly;

                        if (oppositionIsInFormation) //    If you go into formation you are not in position / time to stop the opposition from retreating                   
                        {
                            retreatSuccessful = true;
                        }
                        else if (oppositionIsRushing && (weaponRatio > 6)) // if you rush and or outgun the retreater they are less likely to get away
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 35));

                        }
                        else if ((oppositionIsRushing && (weaponRatio <= 6 && weaponRatio > 3)) || (weaponRatio > 6))
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 20));
                        }
                        else if ((oppositionIsRushing && (weaponRatio <= 3 && weaponRatio > 1)) || (weaponRatio >3))
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 10));
                        }
                        else if ((weaponRatio <= 3 && weaponRatio > 1))
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)(BaseChanceToRetreat * 100));
                        }
                        else if (weaponRatio <= 1 && weaponRatio > 0.5m)
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)(BaseChanceToRetreat * 100) + 20);
                        }
                        else
                        {
                            retreatSuccessful = true;
                        }
                        {
                            //retreatSuccessful = (chanceToRetreat >= (int)((BaseChanceToRetreat * 100) + (int)(weaponRatio/1000)));
                            GameLog.Print("retreting ship ={0} {1}: weaponRatio {2} chance to retreat {3}", _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat);
                        }
                        if (!retreatSuccessful && oppositionIsRushing)
                        {
                            // do something like cut hull to 0.5 of current value attackingShip.HullIntegrity = attackingShip.HullIntegrity/2;
                        }
                        //Perform the retreat
                        if (retreatSuccessful)
                        {


                            if (_traceCombatEngine)
                            {
                                GameLog.Print("{0} {1} successfully retreated...", _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Name);
                            }
                            if (!ownerAssets.EscapedShips.Contains(_combatShips[i].Item1))
                            {
                                ownerAssets.EscapedShips.Add(_combatShips[i].Item1);
                            }
                            if (_combatShips[i].Item1.Source.IsCombatant)
                            {
                                ownerAssets.CombatShips.Remove(_combatShips[i].Item1);
                            }
                            else
                            {
                                ownerAssets.NonCombatShips.Remove(_combatShips[i].Item1);
                            }

                            _combatShips.Remove(_combatShips[i]);

                        }
                        break;

                    case CombatOrder.Standby:
                        if (_traceCombatEngine)
                        {
                            GameLog.Print("{0} {1} standing by...", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID);
                        }
                        break;

                    case CombatOrder.Hail:
                        if (_traceCombatEngine)
                        {
                            GameLog.Print("{0} {1} hailing...", _combatShips[i].Item1.Name, _combatShips[i].Item1.Source.ObjectID);
                        }
                        break;

                }
            }

            //Make sure that the station has a go at the enemy too
            if ((_combatStation != null) && !_combatStation.Item1.IsDestroyed)
            {
                var order = GetOrder(_combatStation.Item1.Source);

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

            //Decloak any ships
            foreach (var combatShip in _combatShips)
            {
                if (combatShip.Item1.IsCloaked)
                {
                    combatShip.Item1.Decloak();
                }
            }
        }

        private CombatUnit ChooseTarget(CombatUnit attacker)
        {
            if (attacker == null)
            {
                throw new ArgumentNullException();
            }

            var attackerOrder = GetOrder(attacker.Source);
            var attackerShipOwner = attacker.Owner;
            //bool oppositionRushing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Rush));

            if ((attackerOrder == CombatOrder.Hail) || (attackerOrder == CombatOrder.LandTroops) || (attackerOrder == CombatOrder.Retreat)|| (attackerOrder == CombatOrder.Standby))
            {
                throw new ArgumentException("Cannot chose a target for a ship that does not have orders that require a target");
            }

            while (true)
            {
                List<Tuple<CombatUnit, CombatWeapon[]>> oppositionShips = _combatShips.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && !cs.Item1.IsCloaked && !cs.Item1.IsDestroyed && attackerShipOwner != cs.Item1.Owner).ToList();
                switch (attackerOrder)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Formation:
                        bool hasOppositionStation = (_combatStation != null) && !_combatStation.Item1.IsDestroyed && (_combatStation.Item1.Owner != attacker.Owner);
                        //Get a list of all of the opposition ships
                        

                        //Only ships to target
                        if (!hasOppositionStation && (oppositionShips.Count() > 0))
                        {
                            return oppositionShips.First().Item1;
                        }
                        //Has both ships and station to target
                        if (hasOppositionStation && (oppositionShips.Count() > 0))
                        {
                            if (Statistics.Random(4) == 0)
                            {
                                return _combatStation.Item1;
                            }
                            return oppositionShips.First().Item1;
                        }
                        //Only has a station to target
                        if (hasOppositionStation)
                        {
                            return _combatStation.Item1;
                        }
                        //Nothing to target
                        return null;

                    case CombatOrder.Rush:
                        //Get a list of any opposition that are retreating
                        var oppositionRetreating = _combatShips.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (GetOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
                        //If there are any, target it
                        if (oppositionRetreating.Count() > 0)
                        {
                            return oppositionRetreating.First().Item1;
                        }
                        //Othewise target everything else (if anything left)
                        attackerOrder = CombatOrder.Engage;
                        break;

                    case CombatOrder.Transports:
                        //Get a list of all the transports
                        var oppositionTransports = _combatShips.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (cs.Item1.Source.OrbitalDesign.ShipType == "Transport") && !cs.Item1.IsDestroyed);
                        bool oppositionIsInFormation = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Formation));
                        //If there are any, return one as the target
                        if ((oppositionTransports.Count() > 0) && (!oppositionIsInFormation)) // you try to target transports but do not get a clear shot if opposition is in formation
                        {
                            return oppositionTransports.First().Item1;
                        }
                        //Otherwise set order to engage and it will go through again
                        attackerOrder = CombatOrder.Engage;
                        break;
                }
            }
        }

        /// <summary>
        /// Deals damage to the target, and calculates whether the target has been destroyed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="weapon"></param>
        private void PerformAttack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
        {
            int accuracy = (int)(_experienceAccuracy[source.Source.ExperienceRank] * 100);

            if (target.IsDestroyed)
            {
                return;
            }

            if (Statistics.Random(100) >= (100 - accuracy))
            {
                target.TakeDamage(weapon.MaxDamage.CurrentValue);
            }
            weapon.Discharge();

            if (target.IsDestroyed)
            {
                if (_traceCombatEngine)
                {
                    GameLog.Print("{0} {1} was destroyed", target.Source.ObjectID, target.Name);
                }
                CombatAssets targetAssets = GetAssets(target.Owner);
                if (target.Source is Ship)
                {
                    var ownerAssets = GetAssets(source.Owner);
                    var oppositionAssets = GetAssets(target.Owner);

                    if (!oppositionAssets.DestroyedShips.Contains(target))
                    {
                        oppositionAssets.DestroyedShips.Add(target);
                    }
                    if (target.Source.IsCombatant)
                    {
                        oppositionAssets.CombatShips.Remove(target);
                    }
                    else
                    {
                        oppositionAssets.NonCombatShips.Remove(target);
                    }
                    _combatShips.RemoveAll(cs => cs.Item1 == target);
                }
            }
        }
    }
}