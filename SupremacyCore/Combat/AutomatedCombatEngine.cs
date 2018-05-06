// AutomatedCombatEngine.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Types;
using Supremacy.Utility;
using System;
using System.Collections.Generic;


namespace Supremacy.Combat
{
    public sealed class AutomatedCombatEngine : CombatEngine
    {
        private const double BaseChanceToRetreat = 0.25;
        private readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        private readonly List<Tuple<CombatUnit, CombatWeapon[]>> _combatShips;
        private Tuple<CombatUnit, CombatWeapon[]> _combatStation;

        private bool _automatedCombatTracing = true;    // turn to true if you want
        //private bool _automatedCombatTracing = false;    // turn to true if you want

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
            bool combatOccurred;


            if (_combatStation != null)
            {
                //GameLog.Print("_combatStation.Count={0}", _combatStation.Count);
                for (int i = 0; i < _combatStation.Item2.Length; i++)
                    _combatStation.Item2[i].Recharge();
            }

            if (_automatedCombatTracing)    
                GameLog.Print("_combatShips.Count={0}", _combatShips.Count);

            for (int i = 0; i < _combatShips.Count; i++)
            {
                for (int j = 0; j < _combatShips[i].Item2.Length; j++)
                    _combatShips[i].Item2[j].Recharge();
            }

            do
            {
                combatOccurred = false;

                _combatShips.ShuffleInPlace();

                if ((_combatStation!= null) && !_combatStation.Item1.IsDestroyed )
                {
                    CombatUnit target = ChooseTarget(_combatStation.Item1.Owner);  // first fighting station ? ....seems so 
                    if (target != null)
                    {
                        foreach (CombatWeapon weapon in _combatStation.Item2)
                        {
                            if (weapon.CanFire)
                            {
                                Attack(_combatStation.Item1, target, weapon);
                                combatOccurred = true;
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < _combatShips.Count; i++)
                {
                    CombatOrder order = GetOrder(_combatShips[i].Item1.Source);
                    CombatAssets ownerAssets = GetAssets(_combatShips[i].Item1.Owner);
                    CombatUnit target;

                    if (order == CombatOrder.Hail)
                    {
                        continue;
                    }

                    if (order == CombatOrder.Retreat)
                    {
                        int chanceToRetreat = Statistics.Random(10000) % 100;
                        if (chanceToRetreat <= (int) (BaseChanceToRetreat * 100))
                        {
                            ownerAssets.EscapedShips.Add(_combatShips[i].Item1);
                            if (_combatShips[i].Item1.Source.IsCombatant)
                            {
                                ownerAssets.CombatShips.Remove(_combatShips[i].Item1);
                            }
                            else
                            {
                                ownerAssets.NonCombatShips.Remove(_combatShips[i].Item1);
                            }
                            _combatShips.RemoveAt(i--);
                        }
                        continue;
                    }

                    target = ChooseTarget(_combatShips[i].Item1.Owner);

                    if (target == null)
                        continue;

                    foreach (CombatWeapon weapon in _combatShips[i].Item2)
                    {
                        if (weapon.CanFire)
                        {
                            int targetIndex = -1;
                            for (int j = 0; j < _combatShips.Count; j++)
                            {
                                if (_combatShips[j].Item1.Source == target.Source)
                                {
                                    targetIndex = j;
                                    break;
                                }
                            }
                            combatOccurred = true;
                            if (Attack(_combatShips[i].Item1, target, weapon))
                            {
                                if (targetIndex < i)
                                    --i;
                            }
                            break;
                        }
                    }
                }
            } while (combatOccurred);

            // Check for IsAssmilated and in combat with Borg
            for (int i = 0; i < _combatShips.Count; i++)
            {
                CombatAssets ownerBorg = GetAssets(_combatShips[i].Item1.Owner);
                if (ownerBorg.Owner.Name.ToString() != "Borg") continue;

                goto Next;
            
            /* statement "goto Next" is always executed before this commeted-out code, therefore this code is always skiped and dead  
             * --> note that goto Next is not part of the statement "if(ownerBorg.Owner...)"
             * This dead code is commented out to suppress compiler warning about "Unreachable code"
            {
                if (_combatShips[i].First.IsAssimilated == true) // _combatShips{[i].Second.Owner.Name.ToString() == "Borg")
                {

                    CombatAssets assimilatedAssets = GetAssets(_combatShips[i].First.Owner);
                    assimilatedAssets.AssimilatedShips.Add(_combatShips[i].First);

                    if (_automatedCombatTracing == true)
                        GameLog.Print("{0} = combatship name, {1} = ownerID, {2} = Owner.Name", _combatShips[i].First.Name, _combatShips[i].First.OwnerID, _combatShips[i].First.Owner.Name);

                    if (_combatShips[i].First.Source.IsCombatant)
                    {
                        assimilatedAssets.CombatShips.Remove(_combatShips[i].First);
                    }
                    else
                    {
                        assimilatedAssets.NonCombatShips.Remove(_combatShips[i].First);
                    }
                    _combatShips.RemoveAt(i--);
                    goto End;
                }
            }
            */

            Next:
                CombatOrder order = GetOrder(_combatShips[i].Item1.Source);
                if (order == CombatOrder.Retreat)
                {
                    CombatAssets ownerAssets = GetAssets(_combatShips[i].Item1.Owner);
                    ownerAssets.EscapedShips.Add(_combatShips[i].Item1);
                    if (_combatShips[i].Item1.Source.IsCombatant)
                    {
                        ownerAssets.CombatShips.Remove(_combatShips[i].Item1);
                    }
                    else
                    {
                        ownerAssets.NonCombatShips.Remove(_combatShips[i].Item1);
                    }
                    _combatShips.RemoveAt(i--);
                }
                else if (_combatShips[i].Item1.IsCloaked)
                {
                    _combatShips[i].Item1.Decloak();
                }

            }

            if (_automatedCombatTracing)   
                GameLog.Print("ResolveCombatRoundCore is done...");
        }

        private CombatUnit ChooseTarget(Civilization sourceOwner)
        {
            CombatUnit result = null;
            int start = Statistics.Random(_combatShips.Count);
            for (int i = start; i < _combatShips.Count; i++)
            {
                if (CombatHelper.WillEngage(_combatShips[i].Item1.Owner, sourceOwner))
                {
                    if (!_combatShips[i].Item1.IsCloaked || (RoundNumber > 1))
                    {
                        result = _combatShips[i].Item1;
                        break;
                    }
                }
            }
            if (result == null)
            {
                for (int i = 0; i < start; i++)
                {
                    if (CombatHelper.WillEngage(_combatShips[i].Item1.Owner, sourceOwner))
                    {
                        if (!_combatShips[i].Item1.IsCloaked || (RoundNumber > 1))
                        {
                            result = _combatShips[i].Item1;
                            break;
                        }
                    }
                }
            }
            if ((_combatStation != null)
                && !_combatStation.Item1.IsDestroyed
                && (sourceOwner != _combatStation.Item1.Owner)
                && ((result == null) || (Statistics.Random(4) == 0)))
            {
                result = _combatStation.Item1;
            }
            return result;
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
                    if (target.Source.IsCombatant)
                    {
                        for (int i = 0; i < targetAssets.CombatShips.Count; i++)
                        {
                            if (targetAssets.CombatShips[i].Source == target.Source)
                            {
                                targetAssets.DestroyedShips.Add(targetAssets.CombatShips[i]);
                                targetAssets.CombatShips.RemoveAt(i);
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
                        if (_combatShips[i].Item1.Source == target.Source)
                        {
                            _combatShips.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            //return target.IsDestroyed;
            //             if (_automatedCombatTracing == true) ; 
            //GameLog.Print("{0} = target.IsAssmilated, {1} = source.Owner, {2} = target.Owner.Name", target.IsAssimilated, source.Owner.ToString(), target.Owner.Name.ToString());
            if (target.ShieldIntegrity < 90 && source.Owner.Name == "Borg" && target.Owner.Name != "Borg")
            {
                if (_automatedCombatTracing) 
                    GameLog.Print("{0} = target.IsAssmilated, {1} = source.Owner, {2} = target.Owner.Name", target.IsAssimilated, source.Owner.ToString(), target.Owner.Name.ToString());

                CombatAssets targetAssets = GetAssets(target.Owner);
                CombatAssets sourceAssets = GetAssets(source.Owner);
                if (target.Source is Ship)
                {
                    target.Source.Scrap = true;
                    if (target.Source.IsCombatant)
                    {
                        for (int i = 0; i < targetAssets.CombatShips.Count; i++)
                        {
                            if (targetAssets.CombatShips[i].Source == target.Source && target.Source.Name.ToString() != "Borg")
                            {

                                target.Source.Owner = source.Owner;
                                target.Source.OwnerID = source.OwnerID;
                                targetAssets.AssimilatedShips.Add(targetAssets.CombatShips[i]);
                                sourceAssets.CombatShips.Add(targetAssets.CombatShips[i]);

                                if (_automatedCombatTracing)
                                    GameLog.Print("Combate Assimilated target.Source.Name {0}, ID {1}, newOwner.Name {2}", target.Source.Name, target.Source.ObjectID, target.Source.Owner.Name);

                                targetAssets.CombatShips.RemoveAt(i);
                                targetAssets.UpdateAllSources();
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < targetAssets.NonCombatShips.Count; i++)
                        {
                            if (targetAssets.NonCombatShips[i].Source == target.Source && target.Source.Name.ToString() != "Borg")
                            {
                                target.Source.Owner = source.Owner;
                                target.Source.OwnerID = source.OwnerID;
                                targetAssets.AssimilatedShips.Add(targetAssets.NonCombatShips[i]);
                                sourceAssets.CombatShips.Add(targetAssets.CombatShips[i]);

                                if (_automatedCombatTracing) 
                                    GameLog.Print("NonCombate Assimilated target.Source.Name {0}, ID {1}, newOwner.Name {2}", target.Source.Name, target.Source.ObjectID, target.Source.Owner.Name);

                                targetAssets.NonCombatShips.RemoveAt(i);
                                //targetAssets.UpdateAllSources();
                                break;
                            }
                        }
                    }
                    for (int i = 0; i < _combatShips.Count; i++)
                    {
                        if (_combatShips[i].Item1.Source == target.Source)
                        {
                            _combatShips.RemoveAt(i);
                            break;
                        }
                    }
                }
                //return target.IsAssimilated;
            }
            return target.IsDestroyed;

        }
    }
}
