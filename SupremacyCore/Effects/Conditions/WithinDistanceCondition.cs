using System.Collections.Generic;

using Supremacy.Game;
using Supremacy.Universe;

using System.Linq;

namespace Supremacy.Effects
{
    public class WithinDistanceCondition : ConditionBase
    {
        public WithinDistanceCondition(IEffectGroup effectGroup)
            : base(effectGroup) {}

        public int Distance { get; set; }
        public ICondition Condition { get; set; }

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            var condition = this.Condition;
            if (condition == null)
                return ConditionSatisfaction.WillNeverSatisfy;

            var universeObject = target as UniverseObject;
            if (universeObject == null)
                return ConditionSatisfaction.WillNeverSatisfy;

            var candidateSet = new HashSet<object>();

            this.BuildCandidateSet(candidateSet, false, CandidateSelectionMode.Union);
            condition.BuildCandidateSet(candidateSet, false, CandidateSelectionMode.Intersection);

            var maxDistance = this.Distance;

            return candidateSet
                .OfType<UniverseObject>()
                .Where(o => o.DistanceTo(universeObject) <= maxDistance)
                .Max(o => condition.SatisfiedBy(o, relativeSource));
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            candidates.UnionWith(GameContext.Current.Universe.Objects.OfType<UniverseObject>());
        }
    }
}