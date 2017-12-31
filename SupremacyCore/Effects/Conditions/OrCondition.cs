using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Collections;

namespace Supremacy.Effects
{
    public sealed class OrCondition : ConditionBase
    {
        private readonly IIndexedEnumerable<ICondition> _conditions;

        public OrCondition(IEffectGroup effectGroup, IEnumerable<ICondition> conditions)
            : base(effectGroup)
        {
            _conditions = new CollectionBase<ICondition>(conditions.ToList());
        }

        public IIndexedEnumerable<ICondition> Conditions
        {
            get { return _conditions; }
        }

        private static Type GetCommonBaseType(Type xType, Type yType)
        {
            if (xType.IsSubclassOf(yType))
                return yType;
            if (yType.IsSubclassOf(xType))
                return xType;
            if (xType == yType)
                return xType;

            var xBase = xType.BaseType;
            var yBase = yType.BaseType;
            if (xBase != null)
            {
                var res = GetCommonBaseType(xBase, yType);
                if (res != null)
                    return res;
            }

            if (yBase != null)
            {
                var res = GetCommonBaseType(xType, yBase);
                if (res != null)
                    return res;
            }

            return null;
        }

        public override Type TargetTypeRestriction
        {
            get
            {
                var baseRestriction = base.TargetTypeRestriction;

                if (_conditions.Count == 0)
                    return baseRestriction;

                var typeRestriction = _conditions[0].TargetTypeRestriction;

                for (var i = 1; i < _conditions.Count; i++)
                {
                    var nextRestriction = _conditions[i].TargetTypeRestriction;
                    if (!typeRestriction.IsAssignableFrom(nextRestriction))
                    {
                        typeRestriction = GetCommonBaseType(typeRestriction, nextRestriction);
                        if (!typeof(IEffectTarget).IsAssignableFrom(typeRestriction))
                            typeRestriction = typeof(IEffectTarget);
                    }
                }

                return typeRestriction;
            }
        }


        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            var maximumSatisfaction = ConditionSatisfaction.WillNeverSatisfy;
            foreach (var condition in _conditions)
            {
                var satisfaction = condition.SatisfiedBy(target, relativeSource);
                if ((satisfaction == ConditionSatisfaction.WillAlwaysSatisfy) || (satisfaction == ConditionSatisfaction.WillSatisfyThroughTurn))
                    return satisfaction;
                if (satisfaction > maximumSatisfaction)
                    maximumSatisfaction = satisfaction;
            }
            return maximumSatisfaction;
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            if (_conditions.Count == 0)
                return;

            var operandCandidates = new HashSet<object>();

            foreach (var operand in Conditions)
                operand.BuildCandidateSet(operandCandidates, relativeSource, CandidateSelectionMode.Union);

            switch (mode)
            {
                case CandidateSelectionMode.Union:
                    candidates.UnionWith(operandCandidates);
                    return;

                case CandidateSelectionMode.Intersection:
                    candidates.IntersectWith(operandCandidates);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}