using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Types;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Diplomacy.Visitors
{
    public class AcceptProposalVisitor : ProposalVisitor
    {
        public static readonly object TransferredColoniesDataKey = new NamedGuid(new Guid("2BDDF322-714E-46AF-B7A1-6D337DE9956B"), "TransferredColonies");

        private readonly IProposal _proposal;
        private readonly Dictionary<object, object> _agreementData;

        private AcceptProposalVisitor([NotNull] IProposal proposal)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            _proposal = proposal;
            _agreementData = new Dictionary<object, object>();
        }

        protected IProposal Proposal
        {
            get { return _proposal; }
        }

        protected Dictionary<object, object> AgreementData
        {
            get { return _agreementData; }
        }

        public static IAgreement Visit([NotNull] IProposal proposal, int turnAccepted = 0)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");
            GameLog.Client.Diplomacy.DebugFormat("Proposal ACCEPTED: Sender {0} vs {1} for {2}"
                , proposal.Sender.Key
                , proposal.Recipient.Key
                , proposal.Clauses[0].ClauseType
                
                );

            var visitor = new AcceptProposalVisitor(proposal);

            proposal.Accept(visitor);

            var agreement = new NewAgreement(
                proposal,
                turnAccepted == 0 ? GameContext.Current.TurnNumber : turnAccepted,
                visitor._agreementData);

            var diplomat = Diplomat.Get(proposal.Recipient);
            var foreignPower = diplomat.GetForeignPower(proposal.Sender);

            GameContext.Current.AgreementMatrix.AddAgreement(agreement);

            var response = new Response(ResponseType.Accept, proposal);

            GameLog.Core.Diplomacy.DebugFormat("Agreement recipient={0} sender ={1}, turn sent ={2}, clauses ={3} response ={4}",
                agreement.Recipient, agreement.Sender, agreement.Proposal.TurnSent, proposal.Clauses.Count, response.ResponseType.ToString());

            foreignPower.ResponseSent = response;
            foreignPower.UpdateStatus();

            return agreement;
        }

        protected void MoveTrappedShips(Civilization owner)
        {
            var universe = GameContext.Current.Universe;
            var fleets = universe.FindOwned<Fleet>(owner);
            var spaceOwner = owner == Proposal.Sender ? Proposal.Recipient : Proposal.Sender;
            var sectorClaims = GameContext.Current.SectorClaims;

            var fleetsToMove = new List<Fleet>();

            foreach (var fleet in fleets)
            {
                var sectorOwner = fleet.Sector.Owner;
                if (sectorOwner == null)
                    sectorOwner = sectorClaims.GetOwner(fleet.Location);

                if (sectorOwner == spaceOwner)
                    fleetsToMove.Add(fleet);
            }

            foreach (var fleet in fleetsToMove)
            {
                var destination = universe.FindNearestOwned<Colony>(fleet.Location, owner);
                if (destination == null)
                    continue;

                fleet.Route = null;
                fleet.Location = destination.Location;

                // TODO: Add SitRep entry letting the player know [why] the shipped was moved
            }
        }

        #region Overrides of ClauseVisitor

        protected override void VisitOfferWithdrawTroopsClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestWithdrawTroopsClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferStopPiracyClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestStopPiracyClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferBreakAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestBreakAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferGiveCreditsClause(IClause clause)
        {
            // this method call is in CreditObligationFulfillmentVisitor.cs to fulfill transfer of credits
        }
        protected override void VisitRequestGiveCreditsClause(IClause clause) { /* TODO */ }
        //protected override void VisitOfferGiveResourcesClause(IClause clause) { /* TODO */ }
        //protected override void VisitRequestGiveResourcesClause(IClause clause) { /* TODO */ }
        //protected override void VisitOfferMapDataClause(IClause clause) { /* TODO */ }
        //protected override void VisitRequestMapDataClause(IClause clause) { /* TODO */ }
        protected override void VisitOfferHonorMilitaryAgreementClause(IClause clause) { /* TODO */ }
        protected override void VisitRequestHonorMilitaryAgreementClause(IClause clause) { /* TODO */ }
        //protected override void VisitOfferEndEmbargoClause(IClause clause) { /* TODO */ }
        //protected override void VisitRequestEndEmbargoClause(IClause clause) { /* TODO */ }
        
        protected override void VisitWarPactClause(IClause clause)
        {
            var senderDiplomat = Diplomat.Get(Proposal.Sender);
            var recipientDiplomat = Diplomat.Get(Proposal.Recipient);

            var target = clause.Data as Civilization; // target civilization of war pact
            if (target == null)
            {
                GameLog.Client.Diplomacy.ErrorFormat(
                    "Civilization {0} sent a war pact proposal to {1} without a valid target.",
                    senderDiplomat.Owner.ShortName,
                    recipientDiplomat.Owner.ShortName);
                
                return;
            }
            // creat WarPactDitionary entry to call DiplomacyHelper.WarPactDitionaryEntry(senderID + recipientID int, target civilizatioin)
            int[] _key = new[] { Proposal.Sender.CivID, Proposal.Recipient.CivID };
            string _civKeyString = "";
            //string _key = Proposal.Sender.CivID.ToString() + Proposal.Recipient.CivID.ToString();
            //int _civKey = int.Parse(_key);
            DiplomacyHelper.WarPactDictionaryEntry(_civKeyString, target);

            var senderForeignPower = senderDiplomat.GetForeignPower(target);
            if (senderForeignPower.DiplomacyData.Status != ForeignPowerStatus.AtWar)
                senderForeignPower.DeclareWar();

            var recipientForeignPower = recipientDiplomat.GetForeignPower(target);
            if (recipientForeignPower.DiplomacyData.Status != ForeignPowerStatus.AtWar)
                recipientForeignPower.DeclareWar();
        }

        protected override void VisitTreatyCeaseFireClause(IClause clause) { /* TODO */ }
        
        protected override void VisitTreatyNonAggressionClause(IClause clause)
        {
            /*
             * Make sure any ships that would be stranded by the newly sealed borders
             * get sent somewhere safe.
             */

            MoveTrappedShips(Proposal.Sender);
            MoveTrappedShips(Proposal.Recipient);
        }

        protected override void VisitTreatyOpenBordersClause(IClause clause)
        {
            /* TODO */ 
        }
        protected override void VisitTreatyTradePactClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyResearchPactClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyAffiliationClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyDefensiveAllianceClause(IClause clause) { /* TODO */ }
        protected override void VisitTreatyFullAllianceClause(IClause clause) { /* TODO */ }

        protected override void VisitTreatyMembershipClause(IClause clause)
        {
            Civilization empire;
            Civilization member;

            if (Proposal.Sender.IsEmpire)
            {
                empire = Proposal.Sender;
                member = Proposal.Recipient;
            }
            else
            {
                empire = Proposal.Recipient;
                member = Proposal.Sender;
            }

            var transferredColonyIds = new List<int>();
            // Transferr Ship Owner in GameEngine DoDiplomacy
            foreach (var colony in GameContext.Current.Universe.FindOwned<Colony>(member))
            {
                colony.TakeOwnership(empire, false);
                transferredColonyIds.Add(colony.ObjectID);
            }

            AgreementData[TransferredColoniesDataKey] = transferredColonyIds;
        }

        #endregion
    }
}