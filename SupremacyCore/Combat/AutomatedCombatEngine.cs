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
            Dictionary<string, int> empirePowers = new Dictionary<string, int>();

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

                if (!empirePowers.Keys.Contains(combatShip.Item1.Owner.Key))
                {
                    empirePowers[combatShip.Item1.Owner.Key] = 0;
                }

                empirePowers[combatShip.Item1.Owner.Key] += CombatHelper.CalculateOrbitalPower(combatShip.Item1.Source);
            }

            if (_traceCombatEngine)
            {
                foreach (var empires in empirePowers) {
                    GameLog.Print("Strength for {0} = {1}", empires.Key, empires.Value);
                }
            }

            _combatShips.ShuffleInPlace();


            for (int i = 0; i < _combatShips.Count; i++)
            {

                var ownerAssets = GetAssets(_combatShips[i].Item1.Owner);
                var oppositionShips = _combatShips.Where(cs => CombatHelper.WillEngage(_combatShips[i].Item1.Owner, cs.Item1.Owner));
                var friendlyShips = _combatShips.Where(cs => !CombatHelper.WillEngage(_combatShips[i].Item1.Owner, cs.Item1.Owner));
                //GameLog.Print("ownerAssets.Owner {0}", ownerAssets.Owner);


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
                        var target = ChooseTarget(attackingShip);
                        int chanceRushingFormation = Statistics.Random(10000) % 100;
                        bool takeDamageRushingFormation;

                        if (target == null && _traceCombatEngine)
                        {
                            GameLog.Print("No target for {1} {0}", attackingShip.Name, attackingShip.Source.ObjectID);
                        }
                        if (target != null)
                        {
                            if (_traceCombatEngine)
                            {
                                GameLog.Print("Target for {1} {0} is {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Source.ObjectID, target.Name);
                            }

                            // if we rushed a formation we could take damage
                            if ((oppositionIsInFormation) && (order == CombatOrder.Rush))
                            {
                                takeDamageRushingFormation = (chanceRushingFormation >= (int)((BaseChanceToRushFormation * 100)));
                                if (takeDamageRushingFormation)
                                {
                                    attackingShip.TakeDamage(attackingShip.Source.OrbitalDesign.HullStrength / 4);  // 25 % down out of Hullstrength of TechObjectDatabase.xml

                                    if (_traceCombatEngine)
                                        GameLog.Print("...rushed a formation and taking damage: ship {0} {1}: Damage taken {2} HullStrength BEFORE {3}, takeDamageRushingFormation = {4}",
                                        attackingShip.Source.ObjectID, attackingShip.Source.Name, attackingShip.Source.OrbitalDesign.HullStrength / 4, attackingShip.Source.HullStrength, takeDamageRushingFormation);

                                }

                            }
                            bool assimilationSuccessful = false;
                            //If the attacker is Borg, try and assimilate before you try destroying it
                            if (attackingShip.Owner.Name == "Borg" && target.Owner.Name != "Borg")
                            {
                                if (_traceCombatEngine)
                                {
                                    GameLog.Print("{0} attempting assimilation on {1} ", attackingShip.Name, target.Owner.Name);
                                }
                                int chanceToAssimilate = Statistics.Random(10000) % 100;
                                assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                            }

                            //Perform the assimilation, but only on ships
                            if (assimilationSuccessful && target.Source is Ship)
                            {
                                if (_traceCombatEngine)
                                {
                                    GameLog.Print("{0} {1} successfully assimilated {2} {3}", attackingShip.Name, attackingShip.Source.ObjectID, target.Name, target.Source.ObjectID);
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
                            else
                            { 
                            //If we're not assimilating, destroy it instead :)
            
                                if (_traceCombatEngine)
                                {
                                    GameLog.Print("{0} {1} attacking {2} {3}",  attackingShip.Source.ObjectID, attackingShip.Name, target.Source.ObjectID, target.Name);
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

                        decimal weaponRatio = 9; // starting value with no deeper sense

                        List<string> ownEmpires = _combatShips.Where(s =>
                            (s.Item1.Owner.Key == _combatShips[i].Item1.Owner.Key))
                            .Select(s => s.Item1.Owner.Key)
                            .Distinct()
                            .ToList();

                        List<string> friendlyEmpires = _combatShips.Where(s =>
                            (s.Item1.Owner.Key != _combatShips[i].Item1.Owner.Key) &&
                            CombatHelper.WillFightAlongside(s.Item1.Owner, _combatShips[i].Item1.Owner))
                            .Select(s => s.Item1.Owner.Key)
                            .Distinct()
                            .ToList();

                        List<string> hostileEmpires = _combatShips.Where(s =>
                            (s.Item1.Owner.Key != _combatShips[i].Item1.Owner.Key) &&
                            CombatHelper.WillEngage(s.Item1.Owner, _combatShips[i].Item1.Owner))
                            .Select(s => s.Item1.Owner.Key)
                            .Distinct()
                            .ToList();

                        int friendlyWeaponPower = ownEmpires.Sum(e => empirePowers[e]) + friendlyEmpires.Sum(e => empirePowers[e]);
                        int hostileWeaponPower = hostileEmpires.Sum(e => empirePowers[e]);
 
                        weaponRatio = friendlyWeaponPower * 10 / hostileWeaponPower;

                        // just for testing
                        //oppositionIsRushing = true;    ##### // just for testing
                        //oppositionIsInFormation = true;

                        GameLog.Print("Friendly Weapon Power = {0}, Hostile Weapon Power = {1}, Ratio={2}", 
                            friendlyWeaponPower, hostileWeaponPower, weaponRatio);

                        if (oppositionIsInFormation) // If you go into formation you are not in position / time to stop the opposition from retreating                   
                        {
                            retreatSuccessful = true;
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is FORMATION: ship {0} {1}: weaponRatio {2} chance to retreat {3}, oppositionIsRushing = {4}, retreatSuccessful = {5}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, oppositionIsRushing, retreatSuccessful);
                        }
                        else if (oppositionIsRushing && (weaponRatio > 6)) // if you rush and outgun the retreater they are less likely to get away
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 35));   // successful if chance is less than 40 % (BaseChance = 75)
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is: RUSH && weaponRatio > 6: ship {0} {1}: weaponRatio {2}, RANDOM retreat chance {3}, retreat chance limit {4}, RUSH = {5}, retreatSuccessful = {6}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, (int)(BaseChanceToRetreat * 100) -35, oppositionIsRushing, retreatSuccessful);
                        }
                        else if ((oppositionIsRushing && (weaponRatio <= 6 && weaponRatio > 3)) || (weaponRatio > 6))
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 20));  // successful if chance is less than 55 % (BaseChance = 75)
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is: RUSH && (weaponRatio <= 6 && weaponRatio > 3) || (weaponRatio > 6): ship {0} {1}: weaponRatio {2} , RANDOM retreat chance {3}, retreat chance limit {4}, RUSH = {5}, retreatSuccessful = {6}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, (int)(BaseChanceToRetreat * 100) - 20, oppositionIsRushing, retreatSuccessful);
                        }
                        else if ((oppositionIsRushing && (weaponRatio <= 3 && weaponRatio > 1)) || (weaponRatio >3))
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 10));  // successful if chance is less than 65 % (BaseChance = 75)
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is (RUSH && (weaponRatio <= 3 && weaponRatio > 1)) || (weaponRatio >3): ship {0} {1}: weaponRatio {2} , RANDOM retreat chance {3}, retreat chance limit {4}, RUSH = {5}, retreatSuccessful = {6}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, (int)(BaseChanceToRetreat * 100) -10, oppositionIsRushing, retreatSuccessful);
                        }
                        else if ((weaponRatio <= 3 && weaponRatio > 1))
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)(BaseChanceToRetreat * 100));  // successful if chance is less than 75 % (BaseChance = 75)
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is weaponRatio = round about 2: ship {0} {1}: weaponRatio {2}, RANDOM retreat chance {3}, retreat chance limit {4}, RUSH = {5}, retreatSuccessful = {6}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, (int)(BaseChanceToRetreat * 100), oppositionIsRushing, retreatSuccessful);
                        }
                        else if (weaponRatio <= 1 && weaponRatio > -1)  // hitting Zero
                        {
                            retreatSuccessful = (chanceToRetreat <= (int)(BaseChanceToRetreat * 100) + 20);   // successful if chance is less than 95 % (BaseChance = 75)
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is weaponRatio = round about 0: ship {0} {1}: weaponRatio {2}, RANDOM retreat chance {3}, retreat chance limit {4}, RUSH = {5}, retreatSuccessful = {6}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, (int)(BaseChanceToRetreat * 100) + 20, oppositionIsRushing, retreatSuccessful);
                        }
                        else
                        {
                            retreatSuccessful = true;   // / successful in all other cases
                            if (_traceCombatEngine)
                                GameLog.Print("Condition is ELSE (nothing else took place): ship {0} {1}: weaponRatio {2} chance to retreat {3}, oppositionIsRushing = {4}, retreatSuccessful = {5}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, oppositionIsRushing, retreatSuccessful);
                        }

                        if (_traceCombatEngine)
                            GameLog.Print("RETREATING ship {0} {1}: weaponRatio {2} chance to retreat {3}, oppositionIsRushing = {4}, retreatSuccessful = {5}",
                            _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, chanceToRetreat, oppositionIsRushing, retreatSuccessful);

                        var hullIntegrityRetreat = _combatShips[i].Item1.HullIntegrity;

                        // just for testing                 ########
                        // retreatSuccessful = false;

                        if (!retreatSuccessful && oppositionIsRushing)
                        {    // risk damage to hull if you fail to retreat and are being rushed
                            if (_traceCombatEngine)
                                GameLog.Print("Retreat failed and being rushed BEFORE: ship {0} {1}: weaponRatio {2} HullStrength BEFORE = {3}, oppositionIsRushing = {4}, retreatSuccessful(really false?) = {5}",
                                _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, _combatShips[i].Item1.Source.HullStrength, oppositionIsRushing, retreatSuccessful);

                            if (chanceToRetreat >= (int)(BaseChanceToRushFormation * 100) && weaponRatio > 1)
                            {
                                _combatShips[i].Item1.TakeDamage(_combatShips[i].Item1.Source.OrbitalDesign.HullStrength / 2);  // 50 % down out of Hullstrength of TechObjectDatabase.xml

                                if (_traceCombatEngine)
                                    GameLog.Print("Retreat failed and being rushed: ship {0} {1}: weaponRatio {2} HullStrength AFTER {3}, oppositionIsRushing = {4}, retreatSuccessful(really false?) = {5}",
                                    _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, weaponRatio, _combatShips[i].Item1.Source.HullStrength, oppositionIsRushing, retreatSuccessful);
                            }
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