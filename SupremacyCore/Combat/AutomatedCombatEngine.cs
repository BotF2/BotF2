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
using Wintellect.PowerCollections;

namespace Supremacy.Combat
{
    public sealed class AutomatedCombatEngine : CombatEngine
    {
        private const double BaseChanceToRetreat = 0.25;
        private const double BaseChanceToAssimilate = 0.1;
        private readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        private readonly List<Tuple<CombatUnit, CombatWeapon[]>> _combatShips;
        private Tuple<CombatUnit, CombatWeapon[]> _combatStation;

        private bool _automatedCombatTracing = false;

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

            _combatShips = new List<Tuple<CombatUnit, CombatWeapon[]>>();

            foreach (CombatAssets civAssets in Assets)
            {
                if (civAssets.Station != null)
                {
                    _combatStation = new Tuple<CombatUnit, CombatWeapon[]>(
                        civAssets.Station,
                        CombatWeapon.CreateWeapons(civAssets.Station.Source));
                }
                foreach (CombatUnit shipStats in civAssets.CombatShips)
                {
                    _combatShips.Add(new Tuple<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));
                }
                foreach (CombatUnit shipStats in civAssets.NonCombatShips)
                {
                    _combatShips.Add(new Tuple<CombatUnit, CombatWeapon[]>(
                        shipStats,
                        CombatWeapon.CreateWeapons(shipStats.Source)));
                }
            }
        }

        protected override void ResolveCombatRoundCore()
        {
            //Recharge all of the weapons
            if (_combatStation.Item1 != null)
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

            Algorithms.RandomShuffleInPlace(_combatShips);

            foreach (var combatShip in _combatShips)
            {
                var ownerAssets = GetAssets(combatShip.Item1.Owner);
                var oppositionShips = _combatShips.Where(cs => CombatHelper.WillEngage(combatShip.Item1.Owner, cs.Item1.Owner));
                var order = GetOrder(combatShip.Item1.Source);
                switch (order)
                {
                    case CombatOrder.Engage:
                    case CombatOrder.Rush:
                    case CombatOrder.Transports:
                    case CombatOrder.Formation:
                        var target = ChooseTarget(combatShip.Item1);
                        if (target != null)
                        {
                            bool assimilationSuccessful = false;
                            //If the attacker is Borg, try and assimilate before you try destroying it
                            if (combatShip.Item1.Owner.Name == "Borg")
                            {
                                int chanceToAssimilate = Statistics.Random(10000) % 100;
                                assimilationSuccessful = chanceToAssimilate <= (int)(BaseChanceToAssimilate * 100);
                            }

                            //Perform the assimilation, but only on ships
                            if (assimilationSuccessful && target.Source is Ship)
                            {
                                var oppositionAssets = GetAssets(target.Owner);
                                oppositionAssets.AssimilatedShips.Add(target);
                                if (target.Source.IsCombatant)
                                {
                                    oppositionAssets.CombatShips.Remove(target);
                                }
                                else
                                {
                                    oppositionAssets.NonCombatShips.Remove(target);
                                }
                                ownerAssets.UpdateAllSources();
                            }

                            //Otherwise attack as normal
                            else
                            {
                                foreach (var weapon in combatShip.Item2.Where(w => w.CanFire))
                                {
                                    Attack(combatShip.Item1, target, weapon);
                                }
                            }
                        }
                        break;

                    case CombatOrder.Retreat:
                        //Calculate the the odds
                        bool oppositionIsRushing = oppositionShips.Any(os => os.Item1.Source.IsCombatant && (GetOrder(os.Item1.Source) == CombatOrder.Rush));
                        int chanceToRetreat = Statistics.Random(10000) % 100;
                        bool retreatSuccessful;
                        if (oppositionIsRushing)
                        {
                            retreatSuccessful = chanceToRetreat <= (int)((BaseChanceToRetreat * 100) - 10);
                        }
                        else
                        {
                            retreatSuccessful = chanceToRetreat <= (int)(BaseChanceToRetreat * 100);
                        }

                        //Perform the retreat
                        if (retreatSuccessful)
                        {
                            ownerAssets.EscapedShips.Add(combatShip.Item1);
                            if (combatShip.Item1.Source.IsCombatant)
                            {
                                ownerAssets.CombatShips.Remove(combatShip.Item1);
                            }
                            else
                            {
                                ownerAssets.NonCombatShips.Remove(combatShip.Item1);
                            }

                            _combatShips.Remove(combatShip);
                        }
                        break;

                    case CombatOrder.Standby:
                    case CombatOrder.Hail:
                        break;
                }
            }

            //Make sure that the station has a go at the enemy too
            if ((_combatStation.Item1 != null) && !_combatStation.Item1.IsDestroyed)
            {
                CombatUnit target = ChooseTarget(_combatStation.Item1);

                if (target != null)
                {
                    foreach (CombatWeapon weapon in _combatStation.Item2.Where(w => w.CanFire))
                    {
                        Attack(_combatStation.Item1, target, weapon);
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

            if ((attackerOrder == CombatOrder.Hail) || (attackerOrder == CombatOrder.LandTroops) || (attackerOrder == CombatOrder.Retreat) || (attackerOrder == CombatOrder.Standby))
            {
                throw new ArgumentException("Cannot chose a target for a ship that does not have orders that require a target");
            }

            while (true) {
                switch (attackerOrder)
                {
                    case CombatOrder.Engage:
                        bool hasOppositionStation = (_combatStation.Item1 != null) && !_combatStation.Item1.IsDestroyed && (_combatStation.Item1.Owner != attacker.Owner);
                        //Get a list of all of the opposition ships
                        var oppositionTargets = _combatShips.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && !cs.Item1.IsCloaked);
                        //Only ships to target
                        if (!hasOppositionStation && (oppositionTargets.Count() > 0))
                        {
                            return oppositionTargets.First().Item1;
                        }
                        //Has both ships and station to target
                        if (hasOppositionStation && (oppositionTargets.Count() > 0))
                        {
                            if (Statistics.Random(4) == 0)
                            {
                                return _combatStation.Item1;
                            }
                            return oppositionTargets.First().Item1;
                        }
                        //Only has a station to target
                        if (hasOppositionStation)
                        {
                            return _combatStation.Item1;
                        }
                        //Nothing to target
                        return null;

                    case CombatOrder.Formation:
                        break;

                    case CombatOrder.Rush:
                        //Get a list of any opposition that are retreating
                        var oppositionRetreating = _combatShips.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (GetOrder(cs.Item1.Source) == CombatOrder.Retreat));
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
                        var oppositionTransports = _combatShips.Where(cs => CombatHelper.WillEngage(attacker.Owner, cs.Item1.Owner) && (cs.Item1.Source.OrbitalDesign.ShipType == "TRANSPORT"));
                        //If there are any, return one as the target
                        if (oppositionTransports.Count() > 0)
                        {
                            return oppositionTransports.First().Item1;
                        }
                        //Otherwise set order to engage and it will go through again
                        attackerOrder = CombatOrder.Engage;
                        break;
                }
            }
        }

        private bool Attack(CombatUnit source, CombatUnit target, CombatWeapon weapon)
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
                    var ownerAssets = GetAssets(source.Owner);
                    var oppositionAssets = GetAssets(target.Owner);

                    oppositionAssets.DestroyedShips.Add(target);
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
            return target.IsDestroyed;
        }        
    }
}