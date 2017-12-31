using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Collections;
using Supremacy.Game;
using Supremacy.Universe;

namespace Supremacy.Effects
{
    public class IsUniverseObjectTypeCondition : ConditionBase
    {
        private readonly UniverseObjectType _objectType;

        public IsUniverseObjectTypeCondition(IEffectGroup effectGroup, UniverseObjectType objectType)
            : base(effectGroup)
        {
            _objectType = objectType;
        }

        public sealed override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            var universeObject = target as UniverseObject;
            if (universeObject == null)
                return ConditionSatisfaction.WillNeverSatisfy;
            if (universeObject.ObjectType != _objectType)
                return ConditionSatisfaction.WillNeverSatisfy;
            return ConditionSatisfaction.WillAlwaysSatisfy;
        }

        public sealed override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            switch (mode)
            {
                case CandidateSelectionMode.Union:
                    candidates.RemoveWhere(o => !(o is UniverseObject) || ((UniverseObject)o).ObjectType != _objectType);
                    return;

                case CandidateSelectionMode.Intersection:
                    candidates.UnionWith(GameContext.Current.Universe.Objects.Where(o => o.ObjectType == _objectType));
                    break;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}