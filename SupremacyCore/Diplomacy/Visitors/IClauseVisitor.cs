namespace Supremacy.Diplomacy.Visitors
{
    public interface IClauseVisitor
    {
        void VisitOfferWithdrawTroopsClause(IClause clause);
        void VisitRequestWithdrawTroopsClause(IClause clause);
        void VisitOfferStopPiracyClause(IClause clause);
        void VisitRequestStopPiracyClause(IClause clause);
        void VisitOfferBreakAgreementClause(IClause clause);
        void VisitRequestBreakAgreementClause(IClause clause);
        void VisitOfferGiveCreditsClause(IClause clause);
        void VisitRequestGiveCreditsClause(IClause clause);
        void VisitOfferGiveResourcesClause(IClause clause);
        void VisitRequestGiveResourcesClause(IClause clause);
        void VisitOfferMapDataClause(IClause clause);
        void VisitRequestMapDataClause(IClause clause);
        void VisitOfferHonorMilitaryAgreementClause(IClause clause);
        void VisitRequestHonorMilitaryAgreementClause(IClause clause);
        void VisitOfferEndEmbargoClause(IClause clause);
        void VisitRequestEndEmbargoClause(IClause clause);
        void VisitWarPactClause(IClause clause);
        void VisitTreatyCeaseFireClause(IClause clause);
        void VisitTreatyNonAggressionClause(IClause clause);
        void VisitTreatyOpenBordersClause(IClause clause);
        void VisitTreatyTradePactClause(IClause clause);
        void VisitTreatyResearchPactClause(IClause clause);
        void VisitTreatyAffiliationClause(IClause clause);
        void VisitTreatyDefensiveAllianceClause(IClause clause);
        void VisitTreatyFullAllianceClause(IClause clause);
        void VisitTreatyMembershipClause(IClause clause);
    }
}