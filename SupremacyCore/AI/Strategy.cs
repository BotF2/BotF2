// Strategy.cs
//
// Copyright (c) 2008 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq;

using Supremacy.Game;
using Supremacy.Types;

namespace Supremacy.AI
{
    [Serializable]
    public class Strategy
    {
        #region Fields
        private ForceMatch _bombardMatch;
        private ForceMatch _defensiveMatch;
        private int? _defensiveTaskForceCount;
        private Percentage? _deficitSpending;
        private int? _desireAttack;
        private int? _desireAttackColony;
        private int? _desireCredits;
        private int? _desireEnlistFriend;
        private int? _desireGrowth;
        private int? _desireIntimidate;
        private int? _desireMakeFriend;
        private int? _desireTrade;
        private int? _disbandArmyCount;
        private int? _distanceModifierFactor;
        private int? _fearColonyDefense;
        private int? _fearEconomyRank;
        private int? _fearInvasion;
        private int? _fearMilitaryRank;
        private int? _fearRaiding;
        private int? _fearScienceRank;
        private int? _fearSpying;
        private int? _fearTech;
        private StrategyGoalEntry[] _goals;
        private ForceMatch _harassMatch;
        private Percentage? _improveGrowthBonus;
        private Percentage? _improveLargeColonyGrowthBonus;
        private Percentage? _improveProductionBonus;
        private Percentage? _improveResourceBonus;
        private Percentage? _improveSmallColonyGrowthBonus;
        private int? _maxRaidingEvents;
        private Percentage? _maxSupportCostPercent;
        private int? _minColonizeDistance;
        private int? _minColonizeScore;
        private int? _minimumMorale;
        private ForceMatch _offensiveMatch;
        private int? _offensiveTaskForceCount;
        private int? _preemptiveStrikeRegard;
        private int? _raidingMemoryTurns;
        private ReadinessLevel? _readinessLevel;
        private ForceMatch _stealthMatch;
        private bool? _stopBuildingFoodBeforePopulationMaximized;

        [NonSerialized]
        private StrategyGoalEntry[] _goalsResolved;
        #endregion

        #region Constructors
        public Strategy(string key, string parentKey)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("value must be a non-null, non-empty string", nameof(key));
            }

            Key = key;
            ParentKey = string.Empty.Equals(key) ? null : parentKey;
            OffensiveMatch = ForceMatch.Default;
            DefensiveMatch = ForceMatch.Default;
            StealthMatch = ForceMatch.Default;
            BombardMatch = ForceMatch.Default;
            HarassMatch = ForceMatch.Default;
            Goals = new StrategyGoalEntry[0];
        }
        #endregion

        #region Properties and Indexers
        public string Key { get; }
        protected string ParentKey { get; private set; }

        public int? MinimumMorale
        {
            get => _minimumMorale ?? (Parent?.MinimumMorale);
            set => _minimumMorale = value;
        }

        public Percentage? DeficitSpending
        {
            get => _deficitSpending.HasValue ? _deficitSpending : (Parent?.DeficitSpending);
            set => _deficitSpending = value;
        }

        public Percentage? MaxSupportCostPercent
        {
            get => _maxSupportCostPercent ?? (Parent?.MaxSupportCostPercent);
            set => _maxSupportCostPercent = value;
        }

        public ReadinessLevel? ReadinessLevel
        {
            get => _readinessLevel ?? (Parent?.ReadinessLevel);
            set => _readinessLevel = value;
        }

        public Percentage? ImproveProductionBonus
        {
            get => _improveProductionBonus ?? (Parent?.ImproveProductionBonus);
            set => _improveProductionBonus = value;
        }

        public Percentage? ImproveGrowthBonus
        {
            get => _improveGrowthBonus ?? (Parent?.ImproveGrowthBonus);
            set => _improveGrowthBonus = value;
        }

        public Percentage? ImproveResourceBonus
        {
            get => _improveResourceBonus ?? (Parent?.ImproveResourceBonus);
            set => _improveResourceBonus = value;
        }

        public Percentage? ImproveSmallColonyGrowthBonus
        {
            get => _improveSmallColonyGrowthBonus ?? (Parent?.ImproveSmallColonyGrowthBonus);
            set => _improveSmallColonyGrowthBonus = value;
        }

        public Percentage? ImproveLargeColonyGrowthBonus
        {
            get => _improveLargeColonyGrowthBonus ?? (Parent?.ImproveLargeColonyGrowthBonus);
            set => _improveLargeColonyGrowthBonus = value;
        }

        public StrategyGoalEntry[] Goals
        {
            get
            {
                if (Parent == null)
                {
                    return _goals;
                }

                if (_goalsResolved == null)
                {
                    _goalsResolved = Parent.Goals.Concat(_goals).ToArray();
                }

                return _goalsResolved;
            }
            set
            {
                if ((_goals != null) && (value != null) && _goals.SequenceEqual(value))
                {
                    return;
                }

                _goals = value;
                _goalsResolved = null;
            }
        }

        public int? DistanceModifierFactor
        {
            get => _distanceModifierFactor ?? (Parent?.DistanceModifierFactor);
            set => _distanceModifierFactor = value;
        }

        public int? DisbandArmyCount
        {
            get => _disbandArmyCount ?? (Parent?.DisbandArmyCount);
            set => _disbandArmyCount = value;
        }

        public int? MinColonizeDistance
        {
            get => _minColonizeDistance ?? (Parent?.MinColonizeDistance);
            set => _minColonizeDistance = value;
        }

        public int? MinColonizeScore
        {
            get => _minColonizeScore ?? (Parent?.MinColonizeScore);
            set => _minColonizeScore = value;
        }

        public int? OffensiveTaskForceCount
        {
            get => _offensiveTaskForceCount ?? (Parent?.OffensiveTaskForceCount);
            set => _offensiveTaskForceCount = value;
        }

        public int? DefensiveTaskForceCount
        {
            get => _defensiveTaskForceCount ?? (Parent?.DefensiveTaskForceCount);
            set => _defensiveTaskForceCount = value;
        }

        public int? FearInvasion
        {
            get => _fearInvasion ?? (Parent?.FearInvasion);
            set => _fearInvasion = value;
        }

        public int? FearColonyDefense
        {
            get => _fearColonyDefense ?? (Parent?.FearColonyDefense);
            set => _fearColonyDefense = value;
        }

        public int? FearRaiding
        {
            get => _fearRaiding ?? (Parent?.FearRaiding);
            set => _fearRaiding = value;
        }

        public int? FearTech
        {
            get => _fearTech ?? (Parent?.FearTech);
            set => _fearTech = value;
        }

        public int? FearSpying
        {
            get => _fearSpying ?? (Parent?.FearSpying);
            set => _fearSpying = value;
        }

        public int? FearScienceRank
        {
            get => _fearScienceRank ?? (Parent?.FearScienceRank);
            set => _fearScienceRank = value;
        }

        public int? FearMilitaryRank
        {
            get => _fearMilitaryRank ?? (Parent?.FearMilitaryRank);
            set => _fearMilitaryRank = value;
        }

        public int? FearEconomyRank
        {
            get => _fearEconomyRank ?? (Parent?.FearEconomyRank);
            set => _fearEconomyRank = value;
        }

        public int? DesireAttack
        {
            get => _desireAttack ?? (Parent?.DesireAttack);
            set => _desireAttack = value;
        }

        public int? DesireAttackColony
        {
            get => _desireAttackColony ?? (Parent?.DesireAttackColony);
            set => _desireAttackColony = value;
        }

        public int? DesireTrade
        {
            get => _desireTrade ?? (Parent?.DesireTrade);
            set => _desireTrade = value;
        }

        public int? DesireGrowth
        {
            get => _desireGrowth ?? (Parent?.DesireGrowth);
            set => _desireGrowth = value;
        }

        public int? DesireCredits
        {
            get => _desireCredits ?? (Parent?.DesireCredits);
            set => _desireCredits = value;
        }

        public int? DesireIntimidate
        {
            get => _desireIntimidate ?? (Parent?.DesireIntimidate);
            set => _desireIntimidate = value;
        }

        public int? DesireMakeFriend
        {
            get => _desireMakeFriend ?? (Parent?.DesireMakeFriend);
            set => _desireMakeFriend = value;
        }

        public int? DesireEnlistFriend
        {
            get => _desireEnlistFriend ?? (Parent?.DesireEnlistFriend);
            set => _desireEnlistFriend = value;
        }

        public int? RaidingMemoryTurns
        {
            get => _raidingMemoryTurns ?? (Parent?.RaidingMemoryTurns);
            set => _raidingMemoryTurns = value;
        }

        public int? MaxRaidingEvents
        {
            get => _maxRaidingEvents ?? (Parent?.MaxRaidingEvents);
            set => _maxRaidingEvents = value;
        }

        public ForceMatch OffensiveMatch
        {
            get => _offensiveMatch ?? (Parent?.OffensiveMatch);
            set => _offensiveMatch = value;
        }

        public ForceMatch DefensiveMatch
        {
            get => _defensiveMatch ?? (Parent?.DefensiveMatch);
            set => _defensiveMatch = value;
        }

        public ForceMatch StealthMatch
        {
            get => _stealthMatch ?? (Parent?.StealthMatch);
            set => _stealthMatch = value;
        }

        public ForceMatch BombardMatch
        {
            get => _bombardMatch ?? (Parent?.BombardMatch);
            set => _bombardMatch = value;
        }

        public ForceMatch HarassMatch
        {
            get => _harassMatch ?? (Parent?.HarassMatch);
            set => _harassMatch = value;
        }

        public int? PreemptiveStrikeRegard
        {
            get => _preemptiveStrikeRegard ?? (Parent?.PreemptiveStrikeRegard);
            set => _preemptiveStrikeRegard = value;
        }

        public bool? StopBuildingFoodBeforePopulationMaximized
        {
            get => _stopBuildingFoodBeforePopulationMaximized ?? (Parent?.StopBuildingFoodBeforePopulationMaximized);
            set => _stopBuildingFoodBeforePopulationMaximized = value;
        }

        public bool? IsInherited => ParentKey != null;

        public Strategy Parent
        {
            get => GameContext.Current.StrategyDatabase[ParentKey];
            set => ParentKey = (value?.Key);
        }
        #endregion
    }

    [Serializable]
    public class StrategyGoalEntry
    {
        #region Constructors
        public StrategyGoalEntry(Goal goal)
        {
            if (goal == null)
            {
                throw new ArgumentNullException(nameof(goal));
            }

            GoalKey = goal.Key;
        }

        public StrategyGoalEntry(string goalKey)
        {
            if (string.IsNullOrEmpty(goalKey))
            {
                throw new ArgumentException("value must be a non-null, non-empty string", nameof(goalKey));
            }

            GoalKey = goalKey;
        }
        #endregion

        #region Properties and Indexers
        public Goal Goal => null;

        public string GoalKey { get; set; }
        public int? Priority { get; set; }
        public int? MaxEvaluations { get; set; }
        public int? MaxExecutions { get; set; }
        public int? ExecutionsPerColony { get; set; }
        public int? EvaluationsPerColony { get; set; }
        public int? PerColony { get; set; }
        #endregion
    }

    [Serializable]
    public class ForceMatch
    {
        #region Constants
        public static readonly ForceMatch Default;
        #endregion

        #region Constructors
        static ForceMatch()
        {
            Default = new ForceMatch();
        }
        #endregion

        #region Properties and Indexers
        public Percentage? Attack { get; set; }
        public Percentage? Defense { get; set; }
        public Percentage? Bombard { get; set; }
        public Percentage? Value { get; set; }
        #endregion
    }
}