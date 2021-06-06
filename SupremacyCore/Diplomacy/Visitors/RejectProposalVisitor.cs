using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Diplomacy.Visitors
{
    public class RejectProposalVisitor : ProposalVisitor
    {
        public static readonly object TransferredColoniesDataKey = new NamedGuid(new Guid("2BDDF322-714E-46AF-B7A1-6D337DE9956B"), "TransferredColonies");

        private readonly IProposal _proposal;
        private readonly Dictionary<object, object> _agreementData;

        private RejectProposalVisitor([NotNull] IProposal proposal)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            _proposal = proposal;
            _agreementData = new Dictionary<object, object>();
        }

        protected IProposal Proposal => _proposal;

        protected Dictionary<object, object> AgreementData => _agreementData;

        public static void Visit([NotNull] IProposal proposal, int turnAccepted = 0)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");


            RejectProposalVisitor visitor = new RejectProposalVisitor(proposal);

            turnAccepted = GameContext.Current.TurnNumber;

            //proposal.Accept(visitor);

            NewAgreement agreement = new NewAgreement(
                proposal,
                turnAccepted,
                visitor._agreementData);

            Diplomat diplomat = Diplomat.Get(proposal.Recipient);
            ForeignPower foreignPower = diplomat.GetForeignPower(proposal.Sender);

            //GameContext.Current.AgreementMatrix.AddAgreement(agreement);

            Response response = new Response(ResponseType.Reject, proposal);
            GameLog.Core.Diplomacy.DebugFormat("RejectProposal from {1} to {0}", diplomat, foreignPower, agreement.Data.ToString());

            foreignPower.ResponseSent = response;
            //foreignPower.UpdateStatus();

            //return agreement;
        }

        #region Overrides of ClauseVisitor

        protected override void VisitOfferWithdrawTroopsClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestWithdrawTroopsClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferStopPiracyClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestStopPiracyClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferBreakAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestBreakAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferGiveCreditsClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestGiveCreditsClause(IClause clause) { /* TODO */ }
        //protected override void VisitOfferGiveResourcesClause(IClause clause) { /* TODO */ }
        //protected override void VisitRequestGiveResourcesClause(IClause clause) { /* TODO */ }
        //protected override void VisitOfferMapDataClause(IClause clause) { /* TODO */ }
        //protected override void VisitRequestMapDataClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferHonorMilitaryAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestHonorMilitaryAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferEndEmbargoClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestEndEmbargoClause(IClause clause) { /* TODO */ }
        
        protected override void VisitWarPactClause(IClause clause)
        {
            //var senderDiplomat = Diplomat.Get(Proposal.Sender);
            //var recipientDiplomat = Diplomat.Get(Proposal.Recipient);

            //var target = clause.Data as Civilization;
            //if (target == null)
            //{
            //    GameLog.Client.Diplomacy.ErrorFormat(
            //        "Civilization {0} sent a war pact proposal to {1} without a valid target.",
            //        senderDiplomat.Owner.ShortName,
            //        recipientDiplomat.Owner.ShortName);
                
            //    return;
            //}

            //var senderForeignPower = senderDiplomat.GetForeignPower(target);
            //if (senderForeignPower.DiplomacyData.Status != ForeignPowerStatus.AtWar)
            //    senderForeignPower.DeclareWar();

            //var recipientForeignPower = recipientDiplomat.GetForeignPower(target);
            //if (recipientForeignPower.DiplomacyData.Status != ForeignPowerStatus.AtWar)
            //    recipientForeignPower.DeclareWar();
        }

        protected override void VisitTreatyCeaseFireClause(IClause clause) { /* TODO */ }
        
        protected override void VisitTreatyNonAggressionClause(IClause clause)
        {
            // what goes here?
        }

        protected override void VisitTreatyOpenBordersClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyTradePactClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyResearchPactClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyAffiliationClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyDefensiveAllianceClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyFullAllianceClause(IClause clause) { /* TODO */ }

        protected override void VisitTreatyMembershipClause(IClause clause)
        {
            //Civilization empire;
            //Civilization member;

            //if (this.Proposal.Sender.IsEmpire)
            //{
            //    empire = this.Proposal.Sender;
            //    member = this.Proposal.Recipient;
            //}
            //else
            //{
            //    empire = this.Proposal.Recipient;
            //    member = this.Proposal.Sender;
            //}

            //var transferredColonyIds = new List<GameObjectID>();

            //foreach (var colony in GameContext.Current.Universe.FindOwned<Colony>(member))
            //{
            //    colony.TakeOwnership(empire, false);
            //    transferredColonyIds.Add(colony.ObjectID);
            //}

            //this.AgreementData[TransferredColoniesDataKey] = transferredColonyIds;
        }

        #endregion
    }
}