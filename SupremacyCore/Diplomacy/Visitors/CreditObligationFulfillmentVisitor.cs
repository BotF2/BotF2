using System;

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Diplomacy.Visitors
{
    public sealed class CreditObligationFulfillmentVisitor : AgreementVisitor
    {
        private readonly IAgreement _agreement;

        private CreditObligationFulfillmentVisitor([NotNull] IAgreement agreement)
        {
            if (agreement == null)
                throw new ArgumentNullException("agreement");

            _agreement = agreement;
        }

        public new static void Visit(IAgreement agreement)
        {
            ((AgreementVisitor)new CreditObligationFulfillmentVisitor(agreement)).Visit(agreement);
        }

        protected override void VisitOfferGiveCreditsClause(IClause clause)
        {
            var creditsData = clause.GetData<CreditsClauseData>();
            if (creditsData == null)
                return;

            TransferCredits(creditsData, _agreement.Proposal.Sender, _agreement.Proposal.Recipient);
        }

        protected override void VisitRequestGiveCreditsClause(IClause clause)
        {
            var creditsData = clause.GetData<CreditsClauseData>();
            if (creditsData == null)
                return;

            TransferCredits(creditsData, _agreement.Proposal.Recipient, _agreement.Proposal.Sender);
        }

        private void TransferCredits(CreditsClauseData creditsData, Civilization sender, Civilization recipient)
        {
            var senderDiplomat = Diplomat.Get(sender);
            var recipientDiplomat = Diplomat.Get(recipient);

            var creditsToTransfer = creditsData.RecurringAmount;

            if (GameContext.Current.TurnNumber == _agreement.StartTurn)
                creditsToTransfer += creditsData.ImmediateAmount;

            //senderDiplomat.OwnerTreasury.Subtract(creditsToTransfer);
            //recipientDiplomat.OwnerTreasury.Add(creditsToTransfer);

            CivilizationManager.For(senderDiplomat.Owner).Credits.AdjustCurrent(-creditsToTransfer);
            CivilizationManager.For(recipientDiplomat.Owner).Credits.AdjustCurrent(creditsToTransfer);
        }
    }
}