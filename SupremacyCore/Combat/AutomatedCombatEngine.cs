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
        private readonly Dictionary<ExperienceRank, double> _experienceAccuracy;
        private readonly List<Pair<CombatUnit, CombatWeapon[]>> _combatShips;
        private Pair<CombatUnit, CombatWeapon[]> _combatStation;

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

            do
            {
                combatOccurred = false;

                Algorithms.RandomShuffleInPlace(_combatShips);

                if ((_combatStation.First != null) && !_combatStation.First.IsDestroyed )
                {
                    CombatUnit target = ChooseTarget(_combatStation.First.Owner);  // first fighting station ? ....seems so 
                    if (target != null)
                    {
                        foreach (CombatWeapon weapon in _combatStation.Second)
                        {
                            if (weapon.CanFire)
                            {
                                Attack(_combatStation.First, target, weapon);
                                combatOccurred = true;
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < _combatShips.Count; i++)
                {
                    CombatOrder order = GetOrder(_combatShips[i].First.Source);
                    CombatAssets ownerAssets = GetAssets(_combatShips[i].First.Owner);
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
                            ownerAssets.EscapedShips.Add(_combatShips[i].First);
                            if (_combatShips[i].First.Source.IsCombatant)
                            {
                                ownerAssets.CombatShips.Remove(_combatShips[i].First);
                            }
                            else
                            {
                                ownerAssets.NonCombatShips.Remove(_combatShips[i].First);
                            }
                            _combatShips.RemoveAt(i--);
                        }
                        continue;
                    }

                    target = ChooseTarget(_combatShips[i].First.Owner);

                    if (target == null)
                        continue;

                    foreach (CombatWeapon weapon in _combatShips[i].Second)
                    {
                        if (weapon.CanFire)
                        {
                            int targetIndex = -1;
                            for (int j = 0; j < _combatShips.Count; j++)
                            {
                                if (_combatShips[j].First.Source == target.Source)
                                {
                                    targetIndex = j;
                                    break;
                                }
                            }
                            combatOccurred = true;
                            if (Attack(_combatShips[i].First, target, weapon))
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
                CombatAssets ownerBorg = GetAssets(_combatShips[i].First.Owner);
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
                CombatOrder order = GetOrder(_combatShips[i].First.Source);
                if (order == CombatOrder.Retreat)
                {
                    CombatAssets ownerAssets = GetAssets(_combatShips[i].First.Owner);
                    ownerAssets.EscapedShips.Add(_combatShips[i].First);
                    if (_combatShips[i].First.Source.IsCombatant)
                    {
                        ownerAssets.CombatShips.Remove(_combatShips[i].First);
                    }
                    else
                    {
                        ownerAssets.NonCombatShips.Remove(_combatShips[i].First);
                    }
                    _combatShips.RemoveAt(i--);
                }
                else if (_combatShips[i].First.IsCloaked)
                {
                    _combatShips[i].First.Decloak();
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
                if (CombatHelper.WillEngage(_combatShips[i].First.Owner, sourceOwner))
                {
                    if (!_combatShips[i].First.IsCloaked || (RoundNumber > 1))
                    {
                        result = _combatShips[i].First;
                        break;
                    }
                }
            }
            if (result == null)
            {
                for (int i = 0; i < start; i++)
                {
                    if (CombatHelper.WillEngage(_combatShips[i].First.Owner, sourceOwner))
                    {
                        if (!_combatShips[i].First.IsCloaked || (RoundNumber > 1))
                        {
                            result = _combatShips[i].First;
                            break;
                        }
                    }
                }
            }
            if ((_combatStation.First != null)
                && !_combatStation.First.IsDestroyed
                && (sourceOwner != _combatStation.First.Owner)
                && ((result == null) || (Statistics.Random(4) == 0)))
            {
                result = _combatStation.First;
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
                        if (_combatShips[i].First.Source == target.Source)
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
                        if (_combatShips[i].First.Source == target.Source)
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
