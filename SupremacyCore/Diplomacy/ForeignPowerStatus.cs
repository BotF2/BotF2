namespace Supremacy.Diplomacy
{
    public enum ForeignPowerStatus : byte
    {
        NoContact = 0,
        OwnerIsSubjugated,
        CounterpartyIsSubjugated,
        AtWar,
        Neutral,
        Peace,
        Friendly,
        Affiliated,
        OwnerIsMember,
        CounterpartyIsMember,
        Allied,
        Self,
        OwnerIsUnreachable,
        CounterpartyIsUnreachable
    }
}