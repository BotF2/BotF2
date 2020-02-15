namespace Supremacy.Diplomacy.Visitors
{
    public interface ISpyOrderAttacker
    {
        //void VisitGift(IProposal proposal);
        void VisitDemand(ISpyOrder spyOrder);   // Steal Credits
        //void VisitExchange(IProposal proposal);
        //void VisitWarPact(IProposal proposal);
        //void VisitTreatyProposal(IProposal proposal);
    }
}