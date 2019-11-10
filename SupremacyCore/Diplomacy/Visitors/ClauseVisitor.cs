namespace Supremacy.Diplomacy.Visitors
{
    public abstract class ClauseVisitor : IClauseVisitor
    {
        protected virtual void VisitOfferWithdrawTroopsClause(IClause clause) { }
        protected virtual void VisitRequestWithdrawTroopsClause(IClause clause) { }
        protected virtual void VisitOfferStopPiracyClause(IClause clause) { }
        protected virtual void VisitRequestStopPiracyClause(IClause clause) { }
        protected virtual void VisitOfferBreakAgreementClause(IClause clause) { }
        protected virtual void VisitRequestBreakAgreementClause(IClause clause) { }
        protected virtual void VisitOfferGiveCreditsClause(IClause clause) { }
        protected virtual void VisitRequestGiveCreditsClause(IClause clause) { }
        protected virtual void VisitOfferGiveResourcesClause(IClause clause) { }
        protected virtual void VisitRequestGiveResourcesClause(IClause clause) { }
        protected virtual void VisitOfferMapDataClause(IClause clause) { }
        protected virtual void VisitRequestMapDataClause(IClause clause) { }
        protected virtual void VisitOfferHonorMilitaryAgreementClause(IClause clause) { }
        protected virtual void VisitRequestHonorMilitaryAgreementClause(IClause clause) { }
        protected virtual void VisitOfferEndEmbargoClause(IClause clause) { }
        protected virtual void VisitRequestEndEmbargoClause(IClause clause) { }
        protected virtual void VisitWarPactClause(IClause clause) { }
        protected virtual void VisitTreatyCeaseFireClause(IClause clause) { }
        protected virtual void VisitTreatyNonAggressionClause(IClause clause) { }
        protected virtual void VisitTreatyOpenBordersClause(IClause clause) { }
        protected virtual void VisitTreatyTradePactClause(IClause clause) { }
        protected virtual void VisitTreatyResearchPactClause(IClause clause) { }
        protected virtual void VisitTreatyAffiliationClause(IClause clause) { }
        protected virtual void VisitTreatyDefensiveAllianceClause(IClause clause) { }
        protected virtual void VisitTreatyFullAllianceClause(IClause clause) { }
        protected virtual void VisitTreatyMembershipClause(IClause clause) { }

        #region Implementation of IClauseVisitor

        void IClauseVisitor.VisitOfferWithdrawTroopsClause(IClause clause)
        {
            VisitOfferWithdrawTroopsClause(clause);
        }

        void IClauseVisitor.VisitRequestWithdrawTroopsClause(IClause clause)
        {
            VisitRequestWithdrawTroopsClause(clause);
        }

        void IClauseVisitor.VisitOfferStopPiracyClause(IClause clause)
        {
            VisitOfferStopPiracyClause(clause);
        }

        void IClauseVisitor.VisitRequestStopPiracyClause(IClause clause)
        {
            VisitRequestStopPiracyClause(clause);
        }

        void IClauseVisitor.VisitOfferBreakAgreementClause(IClause clause)
        {
            VisitOfferBreakAgreementClause(clause);
        }

        void IClauseVisitor.VisitRequestBreakAgreementClause(IClause clause)
        {
            VisitRequestBreakAgreementClause(clause);
        }

        void IClauseVisitor.VisitOfferGiveCreditsClause(IClause clause)
        {
            VisitOfferGiveCreditsClause(clause);
        }

        void IClauseVisitor.VisitRequestGiveCreditsClause(IClause clause)
        {
            VisitRequestGiveCreditsClause(clause);
        }

        void IClauseVisitor.VisitOfferGiveResourcesClause(IClause clause)
        {
            VisitOfferGiveResourcesClause(clause);
        }

        void IClauseVisitor.VisitRequestGiveResourcesClause(IClause clause)
        {
            VisitRequestGiveResourcesClause(clause);
        }

        void IClauseVisitor.VisitOfferMapDataClause(IClause clause)
        {
            VisitOfferMapDataClause(clause);
        }

        void IClauseVisitor.VisitRequestMapDataClause(IClause clause)
        {
            VisitRequestMapDataClause(clause);
        }

        void IClauseVisitor.VisitOfferHonorMilitaryAgreementClause(IClause clause)
        {
            VisitOfferHonorMilitaryAgreementClause(clause);
        }

        void IClauseVisitor.VisitRequestHonorMilitaryAgreementClause(IClause clause)
        {
            VisitRequestHonorMilitaryAgreementClause(clause);
        }

        void IClauseVisitor.VisitOfferEndEmbargoClause(IClause clause)
        {
            VisitOfferEndEmbargoClause(clause);
        }

        void IClauseVisitor.VisitRequestEndEmbargoClause(IClause clause)
        {
            VisitRequestEndEmbargoClause(clause);
        }

        void IClauseVisitor.VisitWarPactClause(IClause clause)
        {
            VisitWarPactClause(clause);
        }

        void IClauseVisitor.VisitTreatyCeaseFireClause(IClause clause)
        {
            VisitTreatyCeaseFireClause(clause);
        }

        void IClauseVisitor.VisitTreatyNonAggressionClause(IClause clause)
        {
            VisitTreatyNonAggressionClause(clause);
        }

        void IClauseVisitor.VisitTreatyOpenBordersClause(IClause clause)
        {
            VisitTreatyOpenBordersClause(clause);
        }

        void IClauseVisitor.VisitTreatyTradePactClause(IClause clause)
        {
            VisitTreatyTradePactClause(clause);
        }

        void IClauseVisitor.VisitTreatyResearchPactClause(IClause clause)
        {
            VisitTreatyResearchPactClause(clause);
        }

        void IClauseVisitor.VisitTreatyAffiliationClause(IClause clause)
        {
            VisitTreatyAffiliationClause(clause);
        }

        void IClauseVisitor.VisitTreatyDefensiveAllianceClause(IClause clause)
        {
            VisitTreatyDefensiveAllianceClause(clause);
        }

        void IClauseVisitor.VisitTreatyFullAllianceClause(IClause clause)
        {
            VisitTreatyFullAllianceClause(clause);
        }

        void IClauseVisitor.VisitTreatyMembershipClause(IClause clause)
        {
            VisitTreatyMembershipClause(clause);
        }

        #endregion
    }
}