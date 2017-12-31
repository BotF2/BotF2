// DProposal.cs[
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Diplomacy
{
    public interface IProposal : IDiplomaticExchange
    {
        TurnNumber TurnSent { get; }
        IIndexedCollection<IClause> Clauses { get; }
        Tone Tone { get; }
    }

    [Serializable]
    public class NewProposal : IProposal
    {
        private readonly TurnNumber _turnSent;
        private readonly GameObjectID _sender;
        private readonly GameObjectID _recipient;
        private readonly CollectionBase<IClause> _clauses;

        public NewProposal(Civilization sender, Civilization recipient, params IClause[] clauses)
            : this(GameContext.Current.TurnNumber, sender, recipient, (IEnumerable<IClause>)clauses) {}

        public NewProposal(Civilization sender, Civilization recipient, IEnumerable<IClause> clauses)
            : this(GameContext.Current.TurnNumber, sender, recipient, clauses) {}

        public NewProposal(TurnNumber turnSent, [NotNull] ICivIdentity sender, [NotNull] ICivIdentity recipient, [NotNull] params IClause[] clauses)
            : this(turnSent, sender, recipient, (IEnumerable<IClause>)clauses) {}

        public NewProposal(TurnNumber turnSent, [NotNull] ICivIdentity sender, [NotNull] ICivIdentity recipient, [NotNull] IEnumerable<IClause> clauses)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (recipient == null)
                throw new ArgumentNullException("recipient");
            if (clauses == null)
                throw new ArgumentNullException("clauses");

            _turnSent = turnSent;
            _sender = sender.CivID;
            _recipient = recipient.CivID;
            _clauses = new CollectionBase<IClause>();

            _clauses.AddRange(clauses);
        }

        #region Implementation of IProposal

        public TurnNumber TurnSent
        {
            get { return _turnSent; }
        }

        public Civilization Sender
        {
            get { return GameContext.Current.Civilizations[_sender]; }
        }

        public Civilization Recipient
        {
            get { return GameContext.Current.Civilizations[_recipient]; }
        }

        public IIndexedCollection<IClause> Clauses
        {
            get { return _clauses; }
        }

        public Tone Tone
        {
            get { return Tone.Calm; }
        }

        #endregion
    }

    public static class ProposalExtensions
    {
        public static IProposal Clone([NotNull] this IProposal proposal)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            return new NewProposal(
                proposal.TurnSent,
                proposal.Sender,
                proposal.Recipient,
                proposal.Clauses.Select(o => o.Clone()));
        }

        public static bool IsGift(this IProposal proposal)
        {
            if (proposal == null)
                return false;
            bool isGift = false;
            foreach (var clause in proposal.Clauses)
            {
                switch (clause.ClauseType)
                {
                    case ClauseType.RequestBreakAgreement:
                    case ClauseType.RequestEndEmbargo:
                    case ClauseType.RequestGiveCredits:
                    case ClauseType.RequestGiveResources:
                    case ClauseType.RequestHonorMilitaryAgreement:
                    case ClauseType.RequestMapData:
                    case ClauseType.RequestStopPiracy:
                    case ClauseType.RequestWithdrawTroops:
                    case ClauseType.TreatyDefensiveAlliance:
                    case ClauseType.TreatyFullAlliance:
                    case ClauseType.TreatyCeaseFire:
                    case ClauseType.TreatyWarPact:
                    case ClauseType.TreatyAffiliation:
                    case ClauseType.TreatyNonAggression:
                    case ClauseType.TreatyOpenBorders:
                    case ClauseType.TreatyResearchPact:
                    case ClauseType.TreatyTradePact:
                    case ClauseType.TreatyMembership:
                        return false;
                    default:
                        isGift = true;
                        break;
                }
            }
            return isGift;
        }

        public static bool IsDemand(this IProposal proposal)
        {
            if (proposal == null)
                return false;
            bool isDemand = false;
            foreach (var clause in proposal.Clauses)
            {
                switch (clause.ClauseType)
                {
                    case ClauseType.OfferBreakAgreement:
                    case ClauseType.OfferEndEmbargo:
                    case ClauseType.OfferGiveCredits:
                    case ClauseType.OfferGiveResources:
                    case ClauseType.OfferHonorMilitaryAgreement:
                    case ClauseType.OfferMapData:
                    case ClauseType.OfferStopPiracy:
                    case ClauseType.OfferWithdrawTroops:
                    case ClauseType.TreatyDefensiveAlliance:
                    case ClauseType.TreatyFullAlliance:
                    case ClauseType.TreatyCeaseFire:
                    case ClauseType.TreatyWarPact:
                    case ClauseType.TreatyAffiliation:
                    case ClauseType.TreatyNonAggression:
                    case ClauseType.TreatyOpenBorders:
                    case ClauseType.TreatyResearchPact:
                    case ClauseType.TreatyTradePact:
                    case ClauseType.TreatyMembership:
                        return false;
                    default:
                        isDemand = true;
                        break;
                }
            }
            return isDemand;
        }

        public static bool HasTreaty(this IProposal proposal)
        {
            if (proposal == null)
                return false;
            foreach (var clause in proposal.Clauses)
            {
                switch (clause.ClauseType)
                {
                    case ClauseType.TreatyDefensiveAlliance:
                    case ClauseType.TreatyFullAlliance:
                    case ClauseType.TreatyCeaseFire:
                    case ClauseType.TreatyWarPact:
                    case ClauseType.TreatyAffiliation:
                    case ClauseType.TreatyNonAggression:
                    case ClauseType.TreatyOpenBorders:
                    case ClauseType.TreatyResearchPact:
                    case ClauseType.TreatyTradePact:
                    case ClauseType.TreatyMembership:
                        return true;
                }
            }
            return false;
        }

        public static bool HasClause(this IProposal proposal, ClauseType clause)
        {
            if (proposal == null)
                return false;

            return proposal.Clauses.Any(c => c.ClauseType == clause);
        }

        public static void Accept([NotNull] this IProposal proposal, [NotNull] IProposalVisitor visitor)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            if (proposal.IsGift())
            {
                visitor.VisitGift(proposal);
                return;
            }

            if (proposal.IsDemand())
            {
                visitor.VisitDemand(proposal);
                return;
            }

            if (proposal.IsWarPact())
            {
                visitor.VisitWarPact(proposal);
                return;
            }

            if (proposal.HasTreaty())
            {
                visitor.VisitTreatyProposal(proposal);
                return;
            }

            visitor.VisitExchange(proposal);
        }
    }
}