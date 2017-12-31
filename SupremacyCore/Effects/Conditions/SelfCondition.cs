using System;
using System.Collections.Generic;

namespace Supremacy.Effects
{
    public sealed class SelfCondition : ConditionBase
    {
        public SelfCondition(IEffectGroup effectGroup)
            : base(effectGroup) { }

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            if (target == relativeSource)
                return ConditionSatisfaction.WillAlwaysSatisfy;
            return ConditionSatisfaction.WillNeverSatisfy;
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            switch (mode)
            {
                case CandidateSelectionMode.Union:
                    candidates.Add(relativeSource);
                    return;

                case CandidateSelectionMode.Intersection:
                    var relativeSourceFoundInCandidateSet = candidates.Contains(relativeSource);
                    candidates.Clear();
                    if (relativeSourceFoundInCandidateSet)
                        candidates.Add(relativeSource);
                    return;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}