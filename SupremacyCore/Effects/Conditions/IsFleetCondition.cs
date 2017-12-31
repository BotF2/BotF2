using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsFleetCondition : IsUniverseObjectTypeCondition
    {
        public IsFleetCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.Fleet) { }
    }
}