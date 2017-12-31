using System;
using System.Collections.Generic;

using Supremacy.Collections;

using System.Linq;

namespace Supremacy.Effects
{
    public sealed class AndCondition : ConditionBase
    {
        private readonly IIndexedEnumerable<ICondition> _conditions;

        public AndCondition(IEffectGroup effectGroup, IEnumerable<ICondition> conditions)
            : base(effectGroup)
        {
            _conditions = new CollectionBase<ICondition>(conditions.ToList());
        }

        public IIndexedEnumerable<ICondition> Conditions
        {
            get { return _conditions; }
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
                    if (typeRestriction.IsAssignableFrom(nextRestriction))
                    {
                        if (nextRestriction == typeRestriction)
                            continue;
                        
                        if (typeRestriction.IsInterface || nextRestriction.IsSubclassOf(typeRestriction))
                        {
                            typeRestriction = nextRestriction;
                            continue;
                        }
                    }
                }

                return typeRestriction;
            }
        }

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            var minimumSatisfaction = (ConditionSatisfaction?)null;
            foreach (var condition in _conditions)
            {
                var satisfaction = condition.SatisfiedBy(target, relativeSource);
                if (satisfaction == ConditionSatisfaction.WillNeverSatisfy)
                    return ConditionSatisfaction.WillNeverSatisfy;
                if (!minimumSatisfaction.HasValue || (satisfaction < minimumSatisfaction.Value))
                    minimumSatisfaction = satisfaction;
            }
            if (!minimumSatisfaction.HasValue)
                return ConditionSatisfaction.WillNeverSatisfy;
            return minimumSatisfaction.Value;
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            if (_conditions.Count == 0)
                return;

            _conditions[0].BuildCandidateSet(candidates, relativeSource, mode);

            for (var i = 1; i < _conditions.Count; i++)
                _conditions[i].BuildCandidateSet(candidates, relativeSource, CandidateSelectionMode.Intersection);
        }
    }

}