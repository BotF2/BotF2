using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class PlanetTypeCondition : ConditionBase
    {
        private readonly PlanetType _planetType;

        public PlanetTypeCondition(IEffectGroup effectGroup, PlanetType planetType)
            : base(effectGroup)
        {
            _planetType = planetType;
        }

        public override ConditionSatisfaction SatisfiedBy(object target, object relativeSource)
        {
            var universeObject = target as UniverseObject;
            if (universeObject == null)
                return ConditionSatisfaction.WillNeverSatisfy;

            var system = universeObject.Sector.System;
            if (system == null)
            {
                if ((universeObject is Ship) || (universeObject is Fleet))
                    return ConditionSatisfaction.WillNotSatisfyThroughTurn;
                return ConditionSatisfaction.WillNeverSatisfy;
            }

            var hasPlanet = system.Planets.Any(o => o.PlanetType == _planetType);
            if ((universeObject is Ship) || (universeObject is Fleet))
                return hasPlanet ? ConditionSatisfaction.WillSatisfyThroughTurn : ConditionSatisfaction.WillNotSatisfyThroughTurn;
            return hasPlanet ? ConditionSatisfaction.WillAlwaysSatisfy : ConditionSatisfaction.WillNeverSatisfy;
        }

        public override void BuildCandidateSet(HashSet<object> candidates, object relativeSource, CandidateSelectionMode mode)
        {
            switch (mode)
            {
                case CandidateSelectionMode.Union:
                    candidates.UnionWith(GameContext.Current.Universe.Objects.Where(o => o.ObjectType == UniverseObjectType.StarSystem));
                    break;

                case CandidateSelectionMode.Intersection:
                    candidates.RemoveWhere(o => !(o is UniverseObject));
                    break;

                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }
    }
}