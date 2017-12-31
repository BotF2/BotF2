using System;
using System.Collections.Generic;

namespace Supremacy.Effects
{
    public sealed class SourceCondition : ConditionBase
    {
        public SourceCondition(IEffectGroup effectGroup)
            : base(effectGroup) {}

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            if (target == this.EffectGroup.Source)
                return ConditionSatisfaction.WillAlwaysSatisfy;
            return ConditionSatisfaction.WillNeverSatisfy;
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            var source = this.EffectGroup.Source;

            switch (mode)
            {
                case CandidateSelectionMode.Union:
                    candidates.Add(source);
                    return;

                case CandidateSelectionMode.Intersection:
                    var sourceFoundInCandidateSet = candidates.Contains(source);
                    candidates.Clear();
                    if (sourceFoundInCandidateSet)
                        candidates.Add(source);
                    return;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}