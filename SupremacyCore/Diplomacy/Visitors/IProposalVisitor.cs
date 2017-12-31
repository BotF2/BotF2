namespace Supremacy.Diplomacy.Visitors
{
    public interface IProposalVisitor
    {
        void VisitGift(IProposal proposal);
        void VisitDemand(IProposal proposal);
        void VisitExchange(IProposal proposal);
        void VisitWarPact(IProposal proposal);
        void VisitTreatyProposal(IProposal proposal);
    }
}