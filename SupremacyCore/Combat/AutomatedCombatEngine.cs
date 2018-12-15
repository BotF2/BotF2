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
        private double countRounds = 1d;
        private bool firstOwnerBadOdds = false;
        private bool firstHostileBadOdds = false;
        private int ownCount = 1;
        private int hostileCount = 1;
        private object firstOwner;
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

        //private void SortCombatUnits(CombatUnit oneUnit, CombatWeapon weapon)
        //{
        //    foreach (var combatent in _combatShips)
        //    {
        //        if (CombatHelper.WillEngage(combatent.Item1.Owner, oneUnit.Owner))
        //        {
        //            OppsotiionCombatShips.Add(combatent);
        //        }
        //        else
        //        {
        //            FriendlyCombatShips.Add(combatent);
        //        }
        //    }
        //}

protected override void ResolveCombatRoundCore()
        {
            //if (_roundNumber > 1) // Sort the ships only in round 1
            // _combatShips.RandomizeInPlace();


            //Array Sorting not working yet, outcommented

            // Unfinished try to resort ships equally. Update xyz Problem with more then 2 empires!

            //if (_combatShips[].IsReadOnly) throw new ArgumentException("List is read-only.", "list");

            //int j = 0;

            //var x = 0;

            //int i2 = 0; // i is already used bellow

            // _combatShipsTemp.Add(_combatShips[0]); // First Ship is copied to Temp to be inserted in first run (initialization)  



            // Suggestion _combatShipsTemp.Add(_combatShips[i2]);


            //if (_roundNumber > 1) // Sort the ships only in round 1

            //{

            //_combatShips.OrderBy(OWner??); Idealy ordered by Owner

            //while (x < _combatShips.Count - 2)

            //{

            //    if (_combatShips[x].Item1.OwnerID != _combatShips[x + 1].Item1.OwnerID)

            //        break;

            //    else

            //        x = x + 1;

            //}

            //while (i2 * 2 < _combatShips.Count - 1)

            //{

            //    if (j % 2 == 0 && j < _combatShips.Count - 1) // j determins wheather its a even or odd slot in the _combatShips

            //    {
            //        _combatShips[j] = _combatShipsTemp[0]; // All even slots are filled with ships of the first empire. Original Positions 1,2,3 until other empire ships start

            //        if (x != 1) // If x = 1, then there are only 2 ships in [0] and [1] so, end of sorting, before we disturb something

            //            _combatShipsTemp[0] = _combatShips[i2 + 1]; // ListField Temp stores the next ship

            //        else

            //            break; // Guess j == .... would have cought that too. But just to be save.
            //    }

            //    else

            //    {
            //        _combatShips[j] = _combatShips[_combatShips.Count - 1 - i2]; // All odd slots are filled with ships of the 2nd empire, Original Position, last, last -1 etc. until first empire ships reached
            //        if (_combatShips.Count - 1 - i2 < 0)
            //        {
            //            break;
            //        }
            //        else
            //        {
            //            _combatShips[_combatShips.Count - 1 - i2] = _combatShips[x + 1 - i2];
            //        }

            //        i2 = i2 + 1;
            //    }
            //    j = j + 1;

            //    if (j == _combatShips.Count - 1 - i2) // Different Empire ships have reached each other

            //        break;

            //}

            //}

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

            //OppositionCombatShips = _combatShips;
            //OppositionCombatShips.Clear();
            //FriendlyCombatShips = _combatShips;
            //FriendlyCombatShips.Clear();
            var lastUnit = _combatShips.LastOrDefault();

            //_combatShipsTemp = _combatShips;

            for (int i = 0; i < _combatShips.Count; i++)
            {
                GameLog.Core.Combat.DebugFormat("sorting combat ships {3} = {0} {1} ({2})",
                    _combatShips[i].Item1.Source.ObjectID, _combatShips[i].Item1.Source.Name, _combatShips[i].Item1.Source.Design, i);
            }
            foreach (var combatent in _combatShips)
            {
                if (CombatHelper.WillEngage(combatent.Item1.Owner, lastUnit.Item1.Owner))
                {
                    
                    OppositionCombatShips.Add(combatent);
                    //_combatShipsTemp.Add(combatent);
                }
                else
                {
                    FriendlyCombatShips.Add(combatent);
                    //_combatShipsTemp.Add(combatent);
                }
            }

            for (int i = 0; i < _combatShips.Count; i++)
            {
                if (i <= OppositionCombatShips.Count - 1)
                    _combatShipsTemp.Add(OppositionCombatShips[i]); // First Ship is copied to Temp to be inserted in first run (initialization)
                if (i <= FriendlyCombatShips.Count - 1)
                    _combatShipsTemp.Add(FriendlyCombatShips[i]);// Second Ship is copied to Temp to be inserted in first run (initialization)
                //if (OppositionCombatShips.Count > FriendlyCombatShips.Count)
                //    _combatShipsTemp.Add(OppositionCombatShips[i]);
                //else
                //    _combatShipsTemp.Add(OppositionCombatShips[i]);
            }

            for (int i = 0; i < _combatShipsTemp.Count; i++)
            {
                GameLog.Core.Combat.DebugFormat("sorting Temp Ships {3} = {0} {1} ({2})",
                    _combatShipsTemp[i].Item1.Source.ObjectID, _combatShipsTemp[i].Item1.Source.Name, _combatShipsTemp[i].Item1.Source.Design, i);
            }

            for (int i = 0; i < _combatShipsTemp.Count; i++)  // random chance for what ship owner is "own" and "opposition"
            {
                var ownerAssets = GetAssets(_combatShipsTemp[i].Item1.Owner);
                var oppositionShips = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, cs.Item1.Owner));
                var friendlyShips = _combatShipsTemp.Where(cs => !CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, cs.Item1.Owner));

                List<string> ownEmpires = _combatShipsTemp.Where(s =>
                    (s.Item1.Owner == _combatShipsTemp[i].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .ToList();

                List<string> friendlyEmpires = _combatShipsTemp.Where(s =>
                    (s.Item1.Owner != _combatShipsTemp[i].Item1.Owner) &&
                    CombatHelper.WillFightAlongside(s.Item1.Owner, _combatShipsTemp[i].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .ToList();

                List<string> hostileEmpires = _combatShipsTemp.Where(s =>
                    (s.Item1.Owner != _combatShipsTemp[i].Item1.Owner) &&
                    CombatHelper.WillEngage(s.Item1.Owner, _combatShipsTemp[i].Item1.Owner))
                    .Select(s => s.Item1.Owner.Key)
                    .ToList();

                if (i == 0)
                {
                    firstOwner = ownerAssets.Owner;
                }

                ownCount = ownEmpires.Count() + friendlyEmpires.Count();
                hostileCount = hostileEmpires.Count();
                if (i == _combatShipsTemp.Count() - 1)
                {
                    if (ownCount < hostileCount - 4)
                    {
                        firstOwnerBadOdds = true; //if there are a lot of targets your "own" ships cannot miss in performAttack
                    }
                    if (ownCount - 4 > hostileCount)
                    {
                        firstHostileBadOdds = true; //if there are a lot of targets your "hostile" ships cannot miss in performAttack
                    }
                }
            }

            for (int i = 0; i < _combatShipsTemp.Count; i++)
            {
                var ownerAssets = GetAssets(_combatShipsTemp[i].Item1.Owner);
                var oppositionShips = _combatShipsTemp.Where(cs => CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, cs.Item1.Owner));
                var friendlyShips = _combatShipsTemp.Where(cs => !CombatHelper.WillEngage(_combatShipsTemp[i].Item1.Owner, cs.Item1.Owner));

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

                //TODO: get intel of system to decamouflage spy ships in system

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

                            else
                            {
                                GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) attacking {3} {4} ({5})", attackingShip.Source.ObjectID, attackingShip.Name, attackingShip.Source.Design, target.Source.ObjectID, target.Name, target.Source.Design);

                                foreach (var weapon in _combatShipsTemp[i].Item2.Where(w => w.CanFire))
                                {
                                    if (!target.IsDestroyed)
                                    {
                                        PerformAttack(attackingShip, target, weapon);
                                    }
                                }
                            }
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
                            return oppositionShips.Last().Item1;
                        }
                        //Has both ships and station to target
                        if (hasOppositionStation && (oppositionShips.Count() > 0))
                        {
                            if (RandomHelper.Random(5) == 0)
                            {
                                return _combatStation.Item1;
                            }
                            return oppositionShips.Last().Item1;
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
        //private void PerformAttack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
        //{
        //    var sourceAccuracy = source.Source.GetAccuracyModifier(); // var? Or double?
        //    var targetDamageControl = target.Source.GetDamageControlModifier();
        //    var ownerAssets = GetAssets(source.Owner);
        //    var oppositionAssets = GetAssets(target.Owner);
        //    int shipdifference = 0; // Update xyz Reduction for ships > same number of opposing fleet
        //    int sameshipnumber = 0;
        //    double howManyShipsHaveFired = 0;
        //    double cycleReduction = 1; // New standard for Damage

        //    if (ownerAssets.CombatShips.Count > oppositionAssets.CombatShips.Count)
        //    {
        //        sameshipnumber = oppositionAssets.CombatShips.Count;
        //        shipdifference = ownerAssets.CombatShips.Count - oppositionAssets.CombatShips.Count;
        //    }
        //    else
        //    {
        //        sameshipnumber = ownerAssets.CombatShips.Count;
        //        shipdifference = oppositionAssets.CombatShips.Count - ownerAssets.CombatShips.Count;
        //    }


        //    if (target.IsDestroyed)
        //    {
        //        return;
        //    }
        //    //if (countRounds != _roundNumber) // if starting a new round rest cycleReduction to 1
        //    //{
        //    //    cycleReduction = 1d;
        //    //    countRounds += 1;
        //    //}


        //    if (targetDamageControl > 1 || targetDamageControl < 0.2)
        //        targetDamageControl = 0.5; // Normalizing target Damage Controle if values are strangly odd

        //    if (sourceAccuracy > 1 || sourceAccuracy < 0.2)
        //        sourceAccuracy = 0.5; // Normalizing target Damage Controle if values are strangly odd


        //    if (source.Owner == ownerAssets.Owner && sameshipnumber == oppositionAssets.CombatShips.Count) // then we are dealing with a ship of the fleet that has more ships
        //    {

        //        //if(source(o => o.IsScout))
        //        //var IsScout = source
        //        //.Where(s => s.Item1.Source.OrbitalDesign.ShipType == "Scout")
        //        //.Where(s => GetOrder(s.Item1.Source) == CombatOrder.Engage);

        //        // fleets.Count(o => o.IsScout)?



        //        ///     W H Y    TARGET NON COMBATTENS??


        //        howManyShipsHaveFired = howManyShipsHaveFired + 1; // Shipnumber or weaponsfire number?)
        //        if (howManyShipsHaveFired > sameshipnumber) // if More ships fired then the number of combat ships both sides have
        //            cycleReduction = 0.70; // Firepower reduced to 70%
        //        if (howManyShipsHaveFired - 1 > sameshipnumber)
        //            cycleReduction = 0.5; // or 50 % if 2 more ships fire
        //        if (howManyShipsHaveFired - 5 > sameshipnumber) // or to 30% if more then 5 ships are fireing then the same number of both fleets
        //            cycleReduction = 0.30;
        //        if (howManyShipsHaveFired - 10 > sameshipnumber) // or to 12% if more then 5 ships are fireing then the same number of both fleets
        //            cycleReduction = 0.12;
        //        targetDamageControl = 1; // If wastly outnumbering the opponent, take 100% damage and deal only 9%
        //                                 // if (source.Source.ShipType = ShipType.Scout)
        //                                 //   cycleReduction = 1;
        //                                 //   if (source.Source.(ShipType.Scout))
        //                                 //     cycleReduction = 1;
        //                                 //if (IsScout. != null)
        //                                 //  cycleReduction = 0.8;

        //    }
        //    else
        //    {
        //        cycleReduction = 1.1; // Update xyz additional 10% firepower
        //        targetDamageControl = 0.4; // Also better DamageControle of the weaker fleet to reduce damage to 40% (instead of 50%)
        //    }






        //    if (RandomHelper.Random(100) <= (100 * sourceAccuracy))  // not every weapons does a hit
        //    {
        //        target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * targetDamageControl * cycleReduction));

        //        GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), targetShields = {6}, hull = {7}",
        //            target.Source.ObjectID, target.Name, target.Source.Design,
        //            (int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy * cycleReduction),
        //            cycleReduction,
        //            sourceAccuracy,
        //            target.ShieldStrength,
        //            target.HullStrength
        //            );



        //    }
        //    weapon.Discharge();

        //    if (target.IsDestroyed)
        //    {
        //        GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) was destroyed", target.Source.ObjectID, target.Name, target.Source.Design);
        //        CombatAssets targetAssets = GetAssets(target.Owner);
        //        if (target.Source is Ship)
        //        {
        //            if (!oppositionAssets.DestroyedShips.Contains(target))
        //            {
        //                oppositionAssets.DestroyedShips.Add(target);
        //            }
        //            if (target.Source.IsCombatant)
        //            {
        //                oppositionAssets.CombatShips.Remove(target);
        //            }
        //            else
        //            {
        //                oppositionAssets.NonCombatShips.Remove(target);
        //            }
        //            _combatShipsTemp.RemoveAll(cs => cs.Item1 == target);
        //        }
        //    }
        //}
        private void PerformAttack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
        {
            var sourceAccuracy = source.Source.GetAccuracyModifier(); // var? Or double?
            var targetDamageControl = target.Source.GetDamageControlModifier();
            var oppositionAssets = GetAssets(target.Owner);
            int pairedCombatents = 1;

            //foreach (var combatent in _combatShips)
            //{
            //    if (CombatHelper.WillEngage(combatent.Item1.Owner, source.Owner))
            //    {
            //        OppsotiionCombatShips.Add(combatent);
            //    }
            //    else
            //    {
            //        FriendlyCombatShips.Add(combatent);
            //    }
            //    if (OppsotiionCombatShips.Count > FriendlyCombatShips.Count)
            //    {
            //        pairedCombatents = FriendlyCombatShips.Count;
            //    }
            //    else
            //    {
            //        pairedCombatents = OppsotiionCombatShips.Count;
            //    }
            //    for (int i = 0; i < pairedCombatents; i++)
            //    {
            //        //if (source.)
            //        //{

            //        //}
            //    }
                if (RandomHelper.Random(100) <= (100 * sourceAccuracy))  // not every weapons does a hit
                {
                    target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy));

                    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), targetDamageControl = {6}, targetShields = {7}, hull = {8}",
                        target.Source.ObjectID, target.Name, target.Source.Design,
                        (int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy * cycleReduction),
                        cycleReduction,
                        sourceAccuracy,
                        targetDamageControl,
                        target.ShieldStrength,
                        target.HullStrength
                        );

                    cycleReduction *= 0.90;
                    if (cycleReduction < 0.6)
                    {
                        cycleReduction = 0.6;
                    }

                }
                weapon.Discharge();
            //}

            //if (sourceAccuracy > 1)
            //    sourceAccuracy = sourceAccuracy / 10;
            //if (sourceAccuracy < 0.2)
            //    sourceAccuracy = 0.5;

            //if (targetDamageControl > 1)
            //    targetDamageControl = targetDamageControl / 10;
            //if (targetDamageControl < 0.2)
            //    targetDamageControl = 0.5;


            //var ownerAssets = GetAssets(source.Owner);
            ////var oppositionAssets = GetAssets(target.Owner);

            //if (target.IsDestroyed)
            //{
            //    return;
            //}
            //if (countRounds != _roundNumber) // if starting a new round rest cycleReduction to 1
            //{
            //    cycleReduction = 1d;
            //    countRounds += 1;
            //}

            //if (firstOwnerBadOdds == true && source.Owner == firstOwner)
            //{
            //    sourceAccuracy = 1;
            //    cycleReduction = 1;
            //    targetDamageControl = 0.25;
            //}
            //if (firstHostileBadOdds == true && source.Owner != firstOwner)
            //{
            //    sourceAccuracy = 1;
            //    cycleReduction = 1;
            //    targetDamageControl = 0.25;
            //}

            //if (firstHostileBadOdds == true && source.Owner == firstOwner)
            //{
            //    targetDamageControl = 0.25;
            //}
            //if (firstOwnerBadOdds == true && source.Owner != firstOwner)
            //{
            //    targetDamageControl = 1;
            //}
            //if (RandomHelper.Random(100) <= (100 * sourceAccuracy))  // not every weapons does a hit
            //{
            //    target.TakeDamage((int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy * cycleReduction * 2));

            //    GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) took damage {3} (cycleReduction = {4}, sourceAccuracy = {5}), targetDamageControl = {6}, targetShields = {7}, hull = {8}",
            //        target.Source.ObjectID, target.Name, target.Source.Design,
            //        (int)(weapon.MaxDamage.CurrentValue * targetDamageControl * sourceAccuracy * cycleReduction),
            //        cycleReduction,
            //        sourceAccuracy,
            //        targetDamageControl,
            //        target.ShieldStrength,
            //        target.HullStrength
            //        );

            //    cycleReduction *= 0.90;
            //    if (cycleReduction < 0.6)
            //    {
            //        cycleReduction = 0.6;
            //    }

            //}
            //weapon.Discharge();

            if (target.IsDestroyed)
            {
                GameLog.Core.Combat.DebugFormat("{0} {1} ({2}) was destroyed", target.Source.ObjectID, target.Name, target.Source.Design);
                CombatAssets targetAssets = GetAssets(target.Owner);
                if (target.Source is Ship)
                {
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
