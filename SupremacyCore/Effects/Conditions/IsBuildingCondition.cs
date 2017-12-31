using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsBuildingCondition : IsUniverseObjectTypeCondition
    {
        public IsBuildingCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.Building) { }
    }
}