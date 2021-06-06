namespace Supremacy.Diplomacy.Visitors
{
    public abstract class ProposalVisitor : ClauseVisitor, IProposalVisitor
    {
        protected virtual void VisitGift(IProposal proposal)
        {
            foreach (IClause clause in proposal.Clauses)
                clause.Accept(this);
        }

        protected virtual void VisitDemand(IProposal proposal)
        {
            foreach (IClause clause in proposal.Clauses)
                clause.Accept(this);
        }

        protected virtual void VisitExchange(IProposal proposal)
        {
            foreach (IClause clause in proposal.Clauses)
                clause.Accept(this);
        }

        protected virtual void VisitWarPact(IProposal proposal)
        {
            foreach (IClause clause in proposal.Clauses)
                clause.Accept(this);
        }

        protected virtual void VisitTreatyProposal(IProposal proposal)
        {
            foreach (IClause clause in proposal.Clauses)
                clause.Accept(this);
        }

        #region Implementation of IProposalVisitor<out IAgreement>

        void IProposalVisitor.VisitDemand(IProposal proposal)
        {
            VisitDemand(proposal);
        }

        void IProposalVisitor.VisitExchange(IProposal proposal)
        {
            VisitExchange(proposal);
        }

        void IProposalVisitor.VisitWarPact(IProposal proposal)
        {
            VisitWarPact(proposal);
        }

        void IProposalVisitor.VisitTreatyProposal(IProposal proposal)
        {
            VisitTreatyProposal(proposal);
        }

        void IProposalVisitor.VisitGift(IProposal proposal)
        {
            VisitGift(proposal);
        }

        #endregion
    }
}