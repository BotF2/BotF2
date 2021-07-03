using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Game;
using Supremacy.Universe;

namespace Supremacy.Diplomacy.Visitors
{
    public class BreakAgreementVisitor : AgreementVisitor
    {
        private readonly IAgreement _agreement;

        private BreakAgreementVisitor([NotNull] IAgreement agreement)
        {
            _agreement = agreement ?? throw new ArgumentNullException("agreement");
        }

        protected IAgreement Agreement => _agreement;

        public static void BreakAgreement([NotNull] IAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement");
            }

            BreakAgreementVisitor visitor = new BreakAgreementVisitor(agreement);

            // TODO: Process penalties for breaking agreements
            visitor.Visit(agreement);

            GameContext.Current.AgreementMatrix.Remove(agreement);
        }

        protected override void VisitTreatyMembershipClause(IClause clause)
        {

            IDictionary<object, object> data = Agreement.Data;
            if (data == null || !data.TryGetValue(AcceptProposalVisitor.TransferredColoniesDataKey, out object dataEntry))
            {
                return;
            }

            if (!(dataEntry is List<int> transferredColonyIds))
            {
                return;
            }

            Entities.Civilization sender = Agreement.Proposal.Sender;
            Entities.Civilization empire = sender.IsEmpire ? sender : Agreement.Proposal.Recipient;
            Entities.Civilization minorRace = sender.IsEmpire ? Agreement.Proposal.Recipient : sender;

            foreach (int transferredColonyId in transferredColonyIds)
            {
                Colony colony = GameContext.Current.Universe.Get<Colony>(transferredColonyId);
                if (colony != null &&
                    colony.Owner == empire &&
                    colony.LastOwnershipChange == Agreement.StartTurn)
                {
                    colony.TakeOwnership(minorRace, false);
                }
            }
        }
    }
}