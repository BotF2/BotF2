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
        bool HeroShip = false;
        private object firstOwner;
        private int wearkerSide =0; // 0= no bigger ships counts, 1= First Friendly side bigger, 2 Oppostion side bigger
        private List<Tuple<CombatUnit, CombatWeapon[]>> _friendlyCombatShips;
        private List<Tuple<CombatUnit, CombatWeapon[]>> _oppositionCombatShips;

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



        //private readonly SendCombatUpdateCallback _updateCallback;
        public AutomatedCombatEngine(
            List<CombatAssets> assets,
            SendCombatUpdateCallback updateCallback,
            NotifyCombatEndedCallback combatEndedCallback)
            : base(assets, updateCallback, combatEndedCallback)
        {

        }

        private void CastType(object firstOwner)
        {
            Civilization actualType = (Civilization)firstOwner;
        }

        protected override void ResolveCombatRoundCore()
        {

            int maxScanStrengthOpposition = 0;

            // Scouts, Frigate and cloaked ships have a special chance of retreating BEFORE round 3
            if (_roundNumber < 3)
            {
                var easyRetreatShips = _combatShips
                    .Where(s => s.Item1.IsCloaked == true || (s.Item1.Source.OrbitalDesign.ShipType == "Frigate") || (s.Item1.Source.OrbitalDesign.ShipType == "Scout"))
                    .Where(s => GetOrder(s.Item1.Source) == CombatOrder.Retreat)
                    .ToList();

                foreach (var ship in easyRetreatShips)
                {
                    if (!RandomHelper.Chance(10) && (ship.Item1 != null))
                    {
                        var ownerAssets = GetAssets(ship.Item1.Owner);
                        ownerAssets.EscapedShips.Add(ship.Item1);
                        ownerAssets.CombatShips.Remove(ship.Item1);
                        ownerAssets.NonCombatShips.Remove(ship.Item1);
                        _combatShips.Remove(ship);
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

            OppositionCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            OppositionCombatShips.Clear();

            FriendlyCombatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();
            FriendlyCombatShips.Clear();

            var firstFriendlyUnit = _combatShips.FirstOrDefault();

            for (int i = 0; i < _combatShips.Count; i++)
            {
                GameLog.Core.Combat.DebugFormat("unsorted combat ships {3} = {0} {1} ({2})",
                    _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, _combatShips[i].Item1.Source.Design, i);
            }
            foreach (var combatent in _combatShips)
            {
                if (CombatHelper.WillEngage(combatent.Item1.Owner, firstFriendlyUnit.Item1.Owner))
                {
                    OppositionCombatShips.Add(combatent);
                    OppositionCombatShips.Randomize();
                }
                else
                {
                    FriendlyCombatShips.Add(combatent);
                    FriendlyCombatShips.Randomize();
                }
            }
            if (OppositionCombatShips.Count == 0 || FriendlyCombatShips.Count == 0)
            {
                shipRatio = 1;
                excessShipsStartingAt = 0;
                wearkerSide = 0;
            }
            else
            {
                if (FriendlyCombatShips.Count - OppositionCombatShips.Count > 0)
                {
                    excessShipsStartingAt = OppositionCombatShips.Count * 2;
                    shipRatio = FriendlyCombatShips.Count() / OppositionCombatShips.Count();
                    wearkerSide = 1;
                }

                else
                {
                    excessShipsStartingAt = FriendlyCombatShips.Count * 2;
                    shipRatio = OppositionCombatShips.Count() / FriendlyCombatShips.Count();
                    wearkerSide = 2;
                }
            }
            if (FriendlyCombatShips.Count() == OppositionCombatShips.Count())
                wearkerSide = 0;

            if (shipRatio > 1.5)
            {
                newCycleReduction = 0.25;
            }
            if (shipRatio > 2)
            {
                newCycleReduction = 0.15;
            }
            if (shipRatio > 3)
            {
                newCycleReduction = 0.08;
            }
            if (shipRatio > 10)
            {
                newCycleReduction = 0.05;
            }
            for (int i = 0; i < _combatShips.Count; i++)
            {
                if (i <= FriendlyCombatShips.Count - 1)
                    _combatShipsTemp.Add(FriendlyCombatShips[i]);// First Ship in _combatShipsTemp is Friendly (initialization)

                if (i <= OppositionCombatShips.Count - 1)
                    _combatShipsTemp.Add(OppositionCombatShips[i]); // Second Ship in _combatShipsTemp is opposition (initialization)
            }

            for (int i = 0; i < _combatShipsTemp.Count; i++)
            {
                GameLog.Core.Combat.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
                    _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Name, _combatShipsTemp[i].Item1.Source.Design, i);
            }

            for (int i = 0; i < _combatShipsTemp.Count; i++)
            {
                //GameLog.Core.Combat.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
                //     _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Name, _combatShipsTemp[i].Item1.Source.Design, i);

                var ownerAssets = GetAssets(_combatShipsTemp[i].Item1.Owner);
                var oppositionShips = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, cs.Item1.Owner));
                var friendlyShips = _combatShipsTemp.Where(cs => !CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, cs.Item1.Owner));

                if (i > excessShipsStartingAt)
                    cycleReduction = newCycleReduction;

                List<string> ownEmpires = _combatShipsTemp.Where(s =>
                    (s.Item1.Owner == _combatShipsTemp[i].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .Distinct()
                    .ToList();

                List<string> friendlyEmpires = _combatShipsTemp.Where(s =>
                    (s.Item1.Owner != _combatShipsTemp[i].Item1.Owner) &&
                    CombatHelper.WillFightAlongside(s.Item1.Owner, _combatShipsTemp[i].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .Distinct()
                    .ToList();

                List<string> hostileEmpires = _combatShipsTemp.Where(s =>
                    (s.Item1.Owner != _combatShipsTemp[i].Item1.Owner) &&
                    CombatHelper.WillEngage(s.Item1.Owner, _combatShipsTemp[i].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .Distinct()
                    .ToList();

                firstOwner = _combatShipsTemp[0].Item1.Owner;
                if (CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, _combatShipsTemp[0].Item1.Owner) && _combatShipsTemp[0].Item1.Owner != _combatShipsTemp[i].Item1.Owner)
                {
                    friendlyOwner = false;
                }
                else
                {
                    friendlyOwner = true;
                }

                int friendlyWeaponPower = ownEmpires.Sum(e => _empireStrengths[e]) + friendlyEmpires.Sum(e => _empireStrengths[e]);
                int hostileWeaponPower = hostileEmpires.Sum(e => _empireStrengths[e]);
                int weaponRatio = friendlyWeaponPower * 10 / (hostileWeaponPower + 1);

                //Figure out if any of the opposition ships have sensors powerful enough to penetrate our camo. If so, will be decamo.
                if (oppositionShips.Count() > 0)

                {
                    maxScanStrengthOpposition = oppositionShips.Max(s => s.Item1.Source.OrbitalDesign.ScanStrength);

                    if (_combatShipsTemp[i].Item1.IsCamouflaged && _combatShipsTemp[i].Item1.CamouflagedStrength < maxScanStrengthOpposition)
                    {
                        _combatShipsTemp[i].Item1.Decamouflage();
                        GameLog.Core.Combat.DebugFormat("{0} has camou strength {1} vs maxScan {2}",
                            _combatShipsTemp[i].Item1.Name, _combatShipsTemp[i].Item1.CamouflagedStrength, maxScanStrengthOpposition);
                    }
                }

                //TODO: Move this to DiplomacyHelper
                List<string> allEmpires = new List<string>();
                allEmpires.AddRange(ownEmpires);
                allEmpires.AddRange(friendlyEmpires);
                allEmpires.AddRange(hostileEmpires);
                foreach (var firstEmpire in allEmpires.Distinct().ToList())
                {
                    foreach (var secondEmpire in allEmpires.Distinct().ToList())
                    {
                        if (!DiplomacyHelper.IsContactMade(Game.GameContext.Current.Civilizations[firstEmpire], Game.GameContext.Current.Civilizations[secondEmpire]))
                        {
                            DiplomacyHelper.EnsureContact(Game.GameContext.Current.Civilizations[firstEmpire], Game.GameContext.Current.Civilizations[secondEmpire], _combatShipsTemp[0].Item1.Source.Location);
                        }
                    }
                }

                bool oppositionIsRushing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Rush));
                bool oppositionIsInFormation = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Formation));
                bool oppositionIsHailing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Hail));
                bool oppositionIsRetreating = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Retreat));
                bool oppositionIsRaidTransports = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Transports));
                bool oppositionIsEngage = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Engage));

                var order = GetOrder(_combatShipsTemp[i].Item1.Source);
                switch (order)
                {

                    case CombatOrder.Engage:
                    case CombatOrder.Rush:
                    case CombatOrder.Transports:
                    case CombatOrder.Formation:

                        var attackingShip = _combatShipsTemp[i].Item1;
                        var target = ChooseTarget(attackingShip);


                        if (target == null)
                        {
                            GameLog.Core.Combat.DebugFormat("No target for {0} {1} ({2})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design);
                        }
                        if (target != null)
                        {
                            GameLog.Core.Combat.DebugFormat("Target for {0} {1} ({2}) is {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);

                            // If we rushed a formation we could take damage
                            int chanceRushingFormation = RandomHelper.Random(100);
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

                            }
                            else // if not assmilated attack, perform attack fire weapons
                            {
                                //GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);

                                var weapons = _combatShipsTemp[i].Item2.Where(w => w.CanFire);
                                int amountOfWeapons = weapons.Count();
                                var partlyFiring = 0;

                                foreach (var weapon in _combatShipsTemp[i].Item2.Where(w => w.CanFire))
                                {
                                    if (!target.IsDestroyed)
                                    {
                                        // just not firing full fire power of one ship before the other ship is firing, but ..
                                        // but each 2nd Weapon e.g. first 5 Beams than 3 Torpedos
                                        if ((partlyFiring += 1) * 2 < amountOfWeapons)
                                        {
                                            GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5}), amountOfWeapons = {6}, partlyFiring Step {7}",
                                                attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design,
                                                amountOfWeapons, partlyFiring);

                                            PerformAttack(attackingShip, target, weapon);
                                            
                                        }
                                    }
                                }
                                // all weapons fired for current ship i

                            }
                            // now we are outside the if else for combat orders including past assimilation borg not borg 
                        }

                        foreach (var combatShip in _combatShipsTemp)
                        {
                            if (combatShip.Item1.IsDestroyed)
                            {
                                ownerAssets.AssimilatedShips.Remove(target);
                            }
                        }

                        break;

                    case CombatOrder.Retreat:
                        if (WasRetreateSuccessful(_combatShipsTemp[i].Item1, oppositionIsRushing, oppositionIsEngage, oppositionIsInFormation, oppositionIsHailing, oppositionIsRetreating, oppositionIsRaidTransports, weaponRatio))
                        {

                            if (!ownerAssets.EscapedShips.Contains(_combatShipsTemp[i].Item1))
                            {
                                ownerAssets.EscapedShips.Add(_combatShipsTemp[i].Item1);
                            }
                            if (_combatShipsTemp[i].Item1.Source.IsCombatant)
                            {
                                ownerAssets.CombatShips.Remove(_combatShipsTemp[i].Item1);
                            }
                            else
                            {
                                ownerAssets.NonCombatShips.Remove(_combatShipsTemp[i].Item1);
                            }

                            _combatShipsTemp.Remove(_combatShipsTemp[i]);
                        }

                        // Chance of hull damage if you fail to retreat and are being rushed
                        else if (oppositionIsRushing && (weaponRatio > 1))
                        {
                            _combatShipsTemp[i].Item1.TakeDamage(_combatShipsTemp[i].Item1.Source.OrbitalDesign.HullStrength / 2);  // 50 % down out of Hullstrength of TechObjectDatabase.xml

                            GameLog.Core.Combat.DebugFormat("{0} {1} failed to retreat whilst being rushed and took {2} damage to hull ({3} hull left)",
                                _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Name, _combatShipsTemp[i].Item1.Source.OrbitalDesign.HullStrength / 2, _combatShipsTemp[i].Item1.Source.HullStrength);
                        }
                        break;

                    case CombatOrder.Hail:
                        GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) hailing...", _combatShipsTemp[i].Item1.Name, _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Design.Name);
                        break;

                    case CombatOrder.Standby:
                        GameLog.Core.Combat.DebugFormat("{1} {0} ({2}) standing by...", _combatShipsTemp[i].Item1.Name, _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Design.Name);
                        break;

                }
                // now remove destroid ships while inside the _combatShipsTemp looping
                if (_combatShipsTemp[i].Item1.IsDestroyed)
                {
                    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) was destroyed", _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Name, _combatShipsTemp[i].Item1.Source.Design);
                    CombatAssets targetAssets = GetAssets(_combatShipsTemp[i].Item1.Owner);
                    if (_combatShipsTemp[i].Item1.Source is Ship)
                    {
                        //var ownerAssets = GetAssets(_combatShipsTemp[i].Item1.Owner);
                        var oppositionAssets = GetAssets(_combatShipsTemp[i].Item1.Owner);

                        if (!oppositionAssets.DestroyedShips.Contains(_combatShipsTemp[i].Item1))
                        {
                            oppositionAssets.DestroyedShips.Add(_combatShipsTemp[i].Item1);
                        }
                        if (_combatShipsTemp[i].Item1.Source.IsCombatant)
                        {
                            oppositionAssets.CombatShips.Remove(_combatShipsTemp[i].Item1);
                        }
                        else
                        {
                            oppositionAssets.NonCombatShips.Remove(_combatShipsTemp[i].Item1);
                        }
                        _combatShips.RemoveAll(cs => cs.Item1 == _combatShipsTemp[i].Item1);
                    }
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
        }

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

            var attackerOrder = GetOrder(attacker.Source);
            var attackerShipOwner = attacker.Owner;

            if ((attackerOrder == CombatOrder.Hail) || (attackerOrder == CombatOrder.LandTroops) || (attackerOrder == CombatOrder.Retreat) || (attackerOrder == CombatOrder.Standby))
            {
                throw new ArgumentException("Cannot chose a target for a ship that does not have orders that require a target");
            }

            List<Tuple<CombatUnit, CombatWeapon[]>> oppositionShips = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && !cs.Item1.IsCloaked && !cs.Item1.IsDestroyed && attackerShipOwner != cs.Item1.Owner).ToList();
            bool hasOppositionStation = (_combatStation != null) && !_combatStation.Item1.IsDestroyed && (_combatStation.Item1.Owner != attacker.Owner);
            while (true)
            {
                switch (attackerOrder)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Formation:
                        //Only ships to target                     
                        if (!hasOppositionStation && (oppositionShips.Count() > 0))
                        {
                            return oppositionShips.FirstOrDefault().Item1;
                        }
                        //Has both ships and station to target
                        if (hasOppositionStation && (oppositionShips.Count() > 0))
                        {
                            if (RandomHelper.Random(5) == 0)
                            {
                                return _combatStation.Item1;
                            }
                            return oppositionShips.FirstOrDefault().Item1;
                        }
                        //Only has a station to target
                        if (hasOppositionStation)
                        {
                            return _combatStation.Item1;
                        }
                        //Nothing to target
                        return null;

                    case CombatOrder.Rush:
                        //If there are any ships that are retreating, target them
                        var oppositionRetreating = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (GetOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
                        if (oppositionRetreating.Count() > 0)
                        {
                            return oppositionRetreating.First().Item1;
                        }
                        attackerOrder = CombatOrder.Engage;
                        break;

                    case CombatOrder.Transports:
                        //If there are transports and they are not in formation, target them
                        var oppositionTransports = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (cs.Item1.Source.OrbitalDesign.ShipType == "Transport") && !cs.Item1.IsDestroyed);
                        bool oppositionIsInFormation = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Formation));
                        if ((oppositionTransports.Count() > 0) && (!oppositionIsInFormation))
                        {
                            return oppositionTransports.First().Item1;
                        }
                        //If there any ships retreating, target them
                        var oppositionRetreatingRaid = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (GetOrder(cs.Item1.Source) == CombatOrder.Retreat) && !cs.Item1.IsDestroyed);
                        if (oppositionRetreatingRaid.Count() > 0)
                        {
                            return oppositionRetreatingRaid.First().Item1;
                        }
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
        /// 

        private void PerformAttack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
        {
            var sourceAccuracy = source.Source.GetAccuracyModifier(); // var? Or double?
            if (sourceAccuracy > 1)  // if getting a 10 from the table
                sourceAccuracy = sourceAccuracy / 10;

            var targetDamageControl = target.Source.GetDamageControlModifier();
            if (targetDamageControl > 1)  // if getting a 10 from the table
                targetDamageControl = targetDamageControl / 10;
           
            // Federation has 7 Enterprises and 2 other Hero Ships

            // 13 Romulan Hero Ships

            // 12 Klingon Hero Ships

            // 5 Cardassians

            // 1 Dominion

            // 3 Borg Ships

            // Bashir

            if (source.Name.Contains("!"))
                HeroShip = true; // If fireing ship is Hero Ship? 

            if (HeroShip == true)
            {
                sourceAccuracy = sourceAccuracy * 1.2; // add 20% accuracy
                targetDamageControl = 1; // Best Damage control for HeroShips
                HeroShip = false; // reset HeroShip to false
            }

            if (RandomHelper.Random(100) <= (100 * sourceAccuracy))  // not every weapons does a hit
                if (wearkerSide == 1)
                {
                    if (source.Owner != firstOwner || !friendlyOwner)
                    {
                        sourceAccuracy = 1.6 - (newCycleReduction *2);
                    } // First (friend) owner is source owner or performAttack is on a friendlyOwner as source owner call from the _combatShipTemp cycle
                }
                {
                    target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * (1.5 - targetDamageControl) * sourceAccuracy * cycleReduction * 3));

                    if (RandomHelper.Random(100) <= (100 * sourceAccuracy))  // not every weapons gets a hit
                {
                    target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy * cycleReduction * 3));

                    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage *3 of all {3} (cycleReduction = {4}, sourceAccuracy = {5}), targetDamageControl = {6}, targetShields = {7}, hull = {8}",
                        target.Source.ObjectID, target.Name, target.Source.Design,
                        (int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy * cycleReduction),
                        cycleReduction,
                        sourceAccuracy,
                        targetDamageControl,
                        target.ShieldStrength,
                        target.HullStrength
                        );


                    //cycleReduction *= 0.98;
                    //if (cycleReduction < 0.6)
                    //    cycleReduction = 0.6;
                }
                weapon.Discharge();
            }

        }
    }
}
