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
            if (String.IsNullOrEmpty(key))
                throw new ArgumentException("value must be a non-null, non-empty string", "key");
            Key = key;
            ParentKey = String.Empty.Equals(key) ? null : parentKey;
            OffensiveMatch = ForceMatch.Default;
            DefensiveMatch = ForceMatch.Default;
            StealthMatch = ForceMatch.Default;
            BombardMatch = ForceMatch.Default;
            HarassMatch = ForceMatch.Default;
            Goals = new StrategyGoalEntry[0];
        }
        #endregion

        #region Properties and Indexers
        public string Key { get; private set; }
        protected string ParentKey { get; private set; }

        public int? MinimumMorale
        {
            get
            {
                if (_minimumMorale.HasValue)
                    return _minimumMorale;
                if (Parent != null)
                    return Parent.MinimumMorale;
                return null;
            }
            set { _minimumMorale = value; }
        }

        public Percentage? DeficitSpending
        {
            get
            {
                if (_deficitSpending.HasValue)
                    return _deficitSpending;
                if (Parent != null)
                    return Parent.DeficitSpending;
                return null;
            }
            set { _deficitSpending = value; }
        }

        public Percentage? MaxSupportCostPercent
        {
            get
            {
                if (_maxSupportCostPercent.HasValue)
                    return _maxSupportCostPercent;
                if (Parent != null)
                    return Parent.MaxSupportCostPercent;
                return null;
            }
            set { _maxSupportCostPercent = value; }
        }

        public ReadinessLevel? ReadinessLevel
        {
            get
            {
                if (_readinessLevel.HasValue)
                    return _readinessLevel;
                if (Parent != null)
                    return Parent.ReadinessLevel;
                return null;
            }
            set { _readinessLevel = value; }
        }

        public Percentage? ImproveProductionBonus
        {
            get
            {
                if (_improveProductionBonus.HasValue)
                    return _improveProductionBonus;
                if (Parent != null)
                    return Parent.ImproveProductionBonus;
                return null;
            }
            set { _improveProductionBonus = value; }
        }

        public Percentage? ImproveGrowthBonus
        {
            get
            {
                if (_improveGrowthBonus.HasValue)
                    return _improveGrowthBonus;
                if (Parent != null)
                    return Parent.ImproveGrowthBonus;
                return null;
            }
            set { _improveGrowthBonus = value; }
        }

        public Percentage? ImproveResourceBonus
        {
            get
            {
                if (_improveResourceBonus.HasValue)
                    return _improveResourceBonus;
                if (Parent != null)
                    return Parent.ImproveResourceBonus;
                return null;
            }
            set { _improveResourceBonus = value; }
        }

        public Percentage? ImproveSmallColonyGrowthBonus
        {
            get
            {
                if (_improveSmallColonyGrowthBonus.HasValue)
                    return _improveSmallColonyGrowthBonus;
                if (Parent != null)
                    return Parent.ImproveSmallColonyGrowthBonus;
                return null;
            }
            set { _improveSmallColonyGrowthBonus = value; }
        }

        public Percentage? ImproveLargeColonyGrowthBonus
        {
            get
            {
                if (_improveLargeColonyGrowthBonus.HasValue)
                    return _improveLargeColonyGrowthBonus;
                if (Parent != null)
                    return Parent.ImproveLargeColonyGrowthBonus;
                return null;
            }
            set { _improveLargeColonyGrowthBonus = value; }
        }

        public StrategyGoalEntry[] Goals
        {
            get
            {
                if (Parent == null)
                    return _goals;
                if (_goalsResolved == null)
                    _goalsResolved = Parent.Goals.Concat(_goals).ToArray();
                return _goalsResolved;
            }
            set
            {
                if ((_goals != null) && (value != null) && _goals.SequenceEqual(value))
                    return;
                _goals = value;
                _goalsResolved = null;
            }
        }

        public int? DistanceModifierFactor
        {
            get
            {
                if (_distanceModifierFactor.HasValue)
                    return _distanceModifierFactor;
                if (Parent != null)
                    return Parent.DistanceModifierFactor;
                return null;
            }
            set { _distanceModifierFactor = value; }
        }

        public int? DisbandArmyCount
        {
            get
            {
                if (_disbandArmyCount.HasValue)
                    return _disbandArmyCount;
                if (Parent != null)
                    return Parent.DisbandArmyCount;
                return null;
            }
            set { _disbandArmyCount = value; }
        }

        public int? MinColonizeDistance
        {
            get
            {
                if (_minColonizeDistance.HasValue)
                    return _minColonizeDistance;
                if (Parent != null)
                    return Parent.MinColonizeDistance;
                return null;
            }
            set { _minColonizeDistance = value; }
        }

        public int? MinColonizeScore
        {
            get
            {
                if (_minColonizeScore.HasValue)
                    return _minColonizeScore;
                if (Parent != null)
                    return Parent.MinColonizeScore;
                return null;
            }
            set { _minColonizeScore = value; }
        }

        public int? OffensiveTaskForceCount
        {
            get
            {
                if (_offensiveTaskForceCount.HasValue)
                    return _offensiveTaskForceCount;
                if (Parent != null)
                    return Parent.OffensiveTaskForceCount;
                return null;
            }
            set { _offensiveTaskForceCount = value; }
        }

        public int? DefensiveTaskForceCount
        {
            get
            {
                if (_defensiveTaskForceCount.HasValue)
                    return _defensiveTaskForceCount;
                if (Parent != null)
                    return Parent.DefensiveTaskForceCount;
                return null;
            }
            set { _defensiveTaskForceCount = value; }
        }

        public int? FearInvasion
        {
            get
            {
                if (_fearInvasion.HasValue)
                    return _fearInvasion;
                if (Parent != null)
                    return Parent.FearInvasion;
                return null;
            }
            set { _fearInvasion = value; }
        }

        public int? FearColonyDefense
        {
            get
            {
                if (_fearColonyDefense.HasValue)
                    return _fearColonyDefense;
                if (Parent != null)
                    return Parent.FearColonyDefense;
                return null;
            }
            set { _fearColonyDefense = value; }
        }

        public int? FearRaiding
        {
            get
            {
                if (_fearRaiding.HasValue)
                    return _fearRaiding;
                if (Parent != null)
                    return Parent.FearRaiding;
                return null;
            }
            set { _fearRaiding = value; }
        }

        public int? FearTech
        {
            get
            {
                if (_fearTech.HasValue)
                    return _fearTech;
                if (Parent != null)
                    return Parent.FearTech;
                return null;
            }
            set { _fearTech = value; }
        }

        public int? FearSpying
        {
            get
            {
                if (_fearSpying.HasValue)
                    return _fearSpying;
                if (Parent != null)
                    return Parent.FearSpying;
                return null;
            }
            set { _fearSpying = value; }
        }

        public int? FearScienceRank
        {
            get
            {
                if (_fearScienceRank.HasValue)
                    return _fearScienceRank;
                if (Parent != null)
                    return Parent.FearScienceRank;
                return null;
            }
            set { _fearScienceRank = value; }
        }

        public int? FearMilitaryRank
        {
            get
            {
                if (_fearMilitaryRank.HasValue)
                    return _fearMilitaryRank;
                if (Parent != null)
                    return Parent.FearMilitaryRank;
                return null;
            }
            set { _fearMilitaryRank = value; }
        }

        public int? FearEconomyRank
        {
            get
            {
                if (_fearEconomyRank.HasValue)
                    return _fearEconomyRank;
                if (Parent != null)
                    return Parent.FearEconomyRank;
                return null;
            }
            set { _fearEconomyRank = value; }
        }

        public int? DesireAttack
        {
            get
            {
                if (_desireAttack.HasValue)
                    return _desireAttack;
                if (Parent != null)
                    return Parent.DesireAttack;
                return null;
            }
            set { _desireAttack = value; }
        }

        public int? DesireAttackColony
        {
            get
            {
                if (_desireAttackColony.HasValue)
                    return _desireAttackColony;
                if (Parent != null)
                    return Parent.DesireAttackColony;
                return null;
            }
            set { _desireAttackColony = value; }
        }

        public int? DesireTrade
        {
            get
            {
                if (_desireTrade.HasValue)
                    return _desireTrade;
                if (Parent != null)
                    return Parent.DesireTrade;
                return null;
            }
            set { _desireTrade = value; }
        }

        public int? DesireGrowth
        {
            get
            {
                if (_desireGrowth.HasValue)
                    return _desireGrowth;
                if (Parent != null)
                    return Parent.DesireGrowth;
                return null;
            }
            set { _desireGrowth = value; }
        }

        public int? DesireCredits
        {
            get
            {
                if (_desireCredits.HasValue)
                    return _desireCredits;
                if (Parent != null)
                    return Parent.DesireCredits;
                return null;
            }
            set { _desireCredits = value; }
        }

        public int? DesireIntimidate
        {
            get
            {
                if (_desireIntimidate.HasValue)
                    return _desireIntimidate;
                if (Parent != null)
                    return Parent.DesireIntimidate;
                return null;
            }
            set { _desireIntimidate = value; }
        }

        public int? DesireMakeFriend
        {
            get
            {
                if (_desireMakeFriend.HasValue)
                    return _desireMakeFriend;
                if (Parent != null)
                    return Parent.DesireMakeFriend;
                return null;
            }
            set { _desireMakeFriend = value; }
        }

        public int? DesireEnlistFriend
        {
            get
            {
                if (_desireEnlistFriend.HasValue)
                    return _desireEnlistFriend;
                if (Parent != null)
                    return Parent.DesireEnlistFriend;
                return null;
            }
            set { _desireEnlistFriend = value; }
        }

        public int? RaidingMemoryTurns
        {
            get
            {
                if (_raidingMemoryTurns.HasValue)
                    return _raidingMemoryTurns;
                if (Parent != null)
                    return Parent.RaidingMemoryTurns;
                return null;
            }
            set { _raidingMemoryTurns = value; }
        }

        public int? MaxRaidingEvents
        {
            get
            {
                if (_maxRaidingEvents.HasValue)
                    return _maxRaidingEvents;
                if (Parent != null)
                    return Parent.MaxRaidingEvents;
                return null;
            }
            set { _maxRaidingEvents = value; }
        }

        public ForceMatch OffensiveMatch
        {
            get
            {
                if (_offensiveMatch != null)
                    return _offensiveMatch;
                if (Parent != null)
                    return Parent.OffensiveMatch;
                return null;
            }
            set { _offensiveMatch = value; }
        }

        public ForceMatch DefensiveMatch
        {
            get
            {
                if (_defensiveMatch != null)
                    return _defensiveMatch;
                if (Parent != null)
                    return Parent.DefensiveMatch;
                return null;
            }
            set { _defensiveMatch = value; }
        }

        public ForceMatch StealthMatch
        {
            get
            {
                if (_stealthMatch != null)
                    return _stealthMatch;
                if (Parent != null)
                    return Parent.StealthMatch;
                return null;
            }
            set { _stealthMatch = value; }
        }

        public ForceMatch BombardMatch
        {
            get
            {
                if (_bombardMatch != null)
                    return _bombardMatch;
                if (Parent != null)
                    return Parent.BombardMatch;
                return null;
            }
            set { _bombardMatch = value; }
        }

        public ForceMatch HarassMatch
        {
            get
            {
                if (_harassMatch != null)
                    return _harassMatch;
                if (Parent != null)
                    return Parent.HarassMatch;
                return null;
            }
            set { _harassMatch = value; }
        }

        public int? PreemptiveStrikeRegard
        {
            get
            {
                if (_preemptiveStrikeRegard.HasValue)
                    return _preemptiveStrikeRegard;
                if (Parent != null)
                    return Parent.PreemptiveStrikeRegard;
                return null;
            }
            set { _preemptiveStrikeRegard = value; }
        }

        public bool? StopBuildingFoodBeforePopulationMaximized
        {
            get
            {
                if (_stopBuildingFoodBeforePopulationMaximized.HasValue)
                    return _stopBuildingFoodBeforePopulationMaximized;
                if (Parent != null)
                    return Parent.StopBuildingFoodBeforePopulationMaximized;
                return null;
            }
            set { _stopBuildingFoodBeforePopulationMaximized = value; }
        }

        public bool? IsInherited
        {
            get { return (ParentKey != null); }
        }

        public Strategy Parent
        {
            get { return GameContext.Current.StrategyDatabase[ParentKey]; }
            set { ParentKey = (value != null) ? value.Key : null; }
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
                throw new ArgumentNullException("goal");
            GoalKey = goal.Key;
        }

        public StrategyGoalEntry(string goalKey)
        {
            if (String.IsNullOrEmpty(goalKey))
                throw new ArgumentException("value must be a non-null, non-empty string", "goalKey");
            GoalKey = goalKey;
        }
        #endregion

        #region Properties and Indexers
        public Goal Goal
        {
            get { return null; }
        }

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