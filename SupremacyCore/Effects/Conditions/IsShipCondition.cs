using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsShipCondition : IsUniverseObjectTypeCondition
    {
        public IsShipCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.Ship) { }
    }
}