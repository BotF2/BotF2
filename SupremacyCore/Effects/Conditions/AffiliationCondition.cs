using System;
using System.Collections.Generic;

namespace Supremacy.Effects
{
    public enum AffiliationType
    {
        Self,
        AllyOf,
        EnemyOf
    }

    public class AffiliationCondition : ConditionBase
    {
        private readonly AffiliationType _mode;

        public AffiliationCondition(IEffectGroup effectGroup, AffiliationType mode)
            : base(effectGroup)
        {
            _mode = mode;
        }

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            throw new NotImplementedException();
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            throw new NotImplementedException();
        }
    }
}