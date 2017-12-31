using System;
using System.Collections.Generic;

using Supremacy.Buildings;
using Supremacy.Game;

using System.Linq;

using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsActiveCondition : ConditionBase
    {
        public IsActiveCondition(IEffectGroup effectGroup)
            : base(effectGroup) {}

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            var building = target as Building;
            if (building == null)
                return ConditionSatisfaction.WillNeverSatisfy;
            return building.IsActive ? ConditionSatisfaction.Satisfies : ConditionSatisfaction.DoesNotSatisfy;
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            switch (mode)
            {
                case CandidateSelectionMode.Union:
                    candidates.UnionWith(GameContext.Current.Universe.Find<Building>().Cast<object>());
                    return;

                case CandidateSelectionMode.Intersection:
                    candidates.RemoveWhere(o => !(o is Building));
                    return;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}