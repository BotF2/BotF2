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
            _agreement = agreement ?? throw new ArgumentNullException("agreement");
        }

        public static new void Visit(IAgreement agreement)
        {
            ((AgreementVisitor)new CreditObligationFulfillmentVisitor(agreement)).Visit(agreement);
        }

        protected override void VisitOfferGiveCreditsClause(IClause clause)
        {
            CreditsClauseData creditsData = clause.GetData<CreditsClauseData>();
            if (creditsData == null)
            {
                return;
            }

            TransferCredits(creditsData, _agreement.Proposal.Sender, _agreement.Proposal.Recipient);
        }

        protected override void VisitRequestGiveCreditsClause(IClause clause)
        {
            CreditsClauseData creditsData = clause.GetData<CreditsClauseData>();
            if (creditsData == null)
            {
                return;
            }

            TransferCredits(creditsData, _agreement.Proposal.Recipient, _agreement.Proposal.Sender);
        }

        private void TransferCredits(CreditsClauseData creditsData, Civilization sender, Civilization recipient)
        {
            Diplomat senderDiplomat = Diplomat.Get(sender);
            Diplomat recipientDiplomat = Diplomat.Get(recipient);

            int creditsToTransfer = creditsData.RecurringAmount;

            if (GameContext.Current.TurnNumber == _agreement.StartTurn)
            {
                creditsToTransfer += creditsData.ImmediateAmount;
            }

            //senderDiplomat.OwnerTreasury.Subtract(creditsToTransfer);
            //recipientDiplomat.OwnerTreasury.Add(creditsToTransfer);

            _ = CivilizationManager.For(senderDiplomat.Owner).Credits.AdjustCurrent(-creditsToTransfer);
            _ = CivilizationManager.For(recipientDiplomat.Owner).Credits.AdjustCurrent(creditsToTransfer);
        }
    }
}