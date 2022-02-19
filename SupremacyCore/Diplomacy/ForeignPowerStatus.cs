// File:ForeignPowerStatus.cs
namespace Supremacy.Diplomacy
{
    public enum ForeignPowerStatus : byte
    {
        NoContact = 0,
        OwnerIsSubjugated,
        CounterpartyIsSubjugated,
        AtWar,
        Hostile,
        Cold,
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