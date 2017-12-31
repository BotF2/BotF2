using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsShipyardCondition : IsUniverseObjectTypeCondition
    {
        public IsShipyardCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.Shipyard) { }
    }
}