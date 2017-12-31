using Supremacy.Universe;

namespace Supremacy.Effects
{
    public sealed class IsStationCondition : IsUniverseObjectTypeCondition
    {
        public IsStationCondition(IEffectGroup effectGroup)
            : base(effectGroup, UniverseObjectType.Station) { }
    }
}