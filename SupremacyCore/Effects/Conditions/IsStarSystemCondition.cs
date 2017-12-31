using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsStarSystemCondition : IsUniverseObjectTypeCondition
    {
        public IsStarSystemCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.StarSystem) { }
    }
}