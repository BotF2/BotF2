using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace Supremacy.Diplomacy
{
    public enum DiplomacyStringID
    {
        None,
        ProposalLeadIn,
        WarPactLeadIn,
        GiftLeadIn,
        DemandLeadIn,
        ExchangeLeadIn,
        ProposalOffersLeadIn,
        ProposalDemandsLeadIn,
        WarPactOffersLeadIn,
        WarPactDemandsLeadIn,
        WarDeclaration,
        WarPactClause,
        CeaseFireClause,
        NonAggressionPactClause,
        OpenBordersClause,
        AffiliationClause,
        DefensiveAllianceClause,
        FullAllianceClause,
        MembershipClause,
        CreditsDemandImmediate,
        CreditsDemandRecurring,
        CreditsDemandImmediateAndRecurring,
        CreditsOfferImmediate,
        CreditsOfferRecurring,
        CreditsOfferImmediateAndRecurring,

        AcceptProposalLeadIn,
        RejectProposalLeadIn,
        AcceptExchangeLeadIn,
        RejectExchangeLeadIn,
        CounterProposalLeadIn,
        AcceptGiftLeadIn,
        AcceptDemandLeadIn,
        RejectDemandLeadIn,

        ActiveAgreementDescriptionTreaty,
        ActiveAgreementDescriptionGift,
        ActiveAgreementDescriptionDemand,
        ActiveAgreementDescriptionExchange,
        ActiveAgreementDescriptionTreatyNoDuration,
        ActiveAgreementDescriptionGiftNoDuration,
        ActiveAgreementDescriptionDemandNoDuration,
        ActiveAgreementDescriptionExchangeNoDuration
    }

    public enum DiplomacySitRepStringKey
    {
        //
        // Summary Text
        //
        CeaseFireProposedSummaryText,
        CeaseFireAcceptedSummaryText,
        CeaseFireRejectedSummaryText,
        WarPactProposedSummaryText,
        WarPactAcceptedSummaryText,
        WarPactRejectedSummaryText,
        NonAggressionPactProposedSummaryText,
        NonAggressionPactAcceptedSummaryText,
        NonAggressionPactRejectedSummaryText,
        OpenBordersProposedSummaryText,
        OpenBordersAcceptedSummaryText,
        OpenBordersRejectedSummaryText,
        AffiliationProposedSummaryText,
        AffiliationAcceptedSummaryText,
        AffiliationRejectedSummaryText,
        DefensiveAllianceProposedSummaryText,
        DefensiveAllianceAcceptedSummaryText,
        DefensiveAllianceRejectedSummaryText,
        FullAllianceProposedSummaryText,
        FullAllianceAcceptedSummaryText,
        FullAllianceRejectedSummaryText,
        MembershipProposedSummaryText,
        MembershipAcceptedSummaryText,
        MembershipRejectedSummaryText,
        WarDeclaredSummaryText,
        ExchangeProposedSummaryText,
        ExchangeAcceptedSummaryText,
        ExchangeRejectedSummaryText,
        GiftOfferedSummaryText,
        TributeDemandedSummaryText,
        TributeAcceptedSummaryText,
        TributeRejectedSummaryText,

        //
        // Detail Text
        //
        NonAggressionPactAcceptedDetailText,
        OpenBordersAcceptedDetailText,
        AffiliationAcceptedDetailText,
        DefensiveAllianceAcceptedDetailText,
        FullAllianceAcceptedDetailText,
        MembershipAcceptedDetailText,
        WarDeclaredDetailText,
    }

    [TypeConverter(typeof(DiplomacyStringKeyConverter))]
    public sealed class DiplomacyStringKey : IEquatable<DiplomacyStringKey>
    {
        private readonly DiplomacyStringID _stringId;

        public DiplomacyStringKey(string civilization, DiplomacyStringID stringId)
        {
            Civilization = civilization;
            _stringId = stringId;
        }

        public string Civilization { get; }

        public DiplomacyStringID StringID => _stringId;

        public bool Equals(DiplomacyStringKey other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other._stringId == _stringId &&
                   string.Equals(other.Civilization, Civilization, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiplomacyStringKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Civilization != null ? Civilization.GetHashCode() : 0) * 397) ^ _stringId.GetHashCode();
            }
        }
    }

    public sealed class DiplomacyStringKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is DiplomacyStringKey key &&
                destinationType == typeof(MarkupExtension))
            {
                return new DiplomacyStringExtension
                {
                    Civilization = key.Civilization,
                    StringID = key.StringID
                };
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [MarkupExtensionReturnType(typeof(DiplomacyStringKey))]
    public sealed class DiplomacyStringExtension : MarkupExtension
    {
        public string Civilization { get; set; }
        public DiplomacyStringID StringID { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new DiplomacyStringKey(Civilization, StringID);
        }
    }
}