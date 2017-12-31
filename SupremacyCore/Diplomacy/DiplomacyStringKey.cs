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
        private readonly string _civilization;
        private readonly DiplomacyStringID _stringId;

        public DiplomacyStringKey(string civilization, DiplomacyStringID stringId)
        {
            _civilization = civilization;
            _stringId = stringId;
        }

        public string Civilization
        {
            get { return _civilization; }
        }

        public DiplomacyStringID StringID
        {
            get { return _stringId; }
        }

        public bool Equals(DiplomacyStringKey other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other._stringId == _stringId &&
                   string.Equals(other._civilization, _civilization, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiplomacyStringKey);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_civilization != null ? _civilization.GetHashCode() : 0) * 397) ^ _stringId.GetHashCode();
            }
        }
    }

    public sealed class DiplomacyStringKeyConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
                return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var key = value as DiplomacyStringKey;
            if (key != null &&
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