using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsColonyCondition : IsUniverseObjectTypeCondition
    {
        public IsColonyCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.Colony) {}
    }
}