// SpyOrder.cs   // before IProposal.cs for Diplo
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
using Supremacy.Utility;


// toDo: create a namespace SpyOperations .. we don't know about and kept it to Diplomacy
namespace Supremacy.Diplomacy
{
    public interface ISpyOrder : IDiplomaticExchange
    {
        int TurnSent { get; }
        IIndexedCollection<IClause> Clauses { get; }
        Tone Tone { get; }
    }

    [Serializable]
    public class NewSpyOrder : ISpyOrder
    {
        private readonly int _turnSent;
        private readonly int _sender;
        private readonly int _recipient;
        private readonly CollectionBase<IClause> _clauses;

        public NewSpyOrder(Civilization sender, Civilization recipient, params IClause[] clauses)
            : this(GameContext.Current.TurnNumber, sender, recipient, (IEnumerable<IClause>)clauses) { }

        public NewSpyOrder(Civilization sender, Civilization recipient, IEnumerable<IClause> clauses)
            : this(GameContext.Current.TurnNumber, sender, recipient, clauses) { }

        public NewSpyOrder(int turnSent, [NotNull] ICivIdentity sender, [NotNull] ICivIdentity recipient, [NotNull] params IClause[] clauses)
            : this(turnSent, sender, recipient, (IEnumerable<IClause>)clauses) { }

        public NewSpyOrder(int turnSent, [NotNull] ICivIdentity sender, [NotNull] ICivIdentity recipient, [NotNull] IEnumerable<IClause> clauses)
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

        #region Implementation of ISpyOrder

        public int TurnSent
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

    public static class SpyOrderExtensions
    {
        public static ISpyOrder Clone([NotNull] this ISpyOrder spyOrder)
        {
            if (spyOrder == null)
                throw new ArgumentNullException("spyOrder");

            return new NewSpyOrder(
                spyOrder.TurnSent,
                spyOrder.Sender,
                spyOrder.Recipient,
                spyOrder.Clauses.Select(o => o.Clone()));
        }

        public static bool IsGift(this ISpyOrder spyOrder)
        {
            if (spyOrder == null)
                return false;
            bool isGift = false;
            foreach (var clause in spyOrder.Clauses)
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

        public static bool IsDemand(this ISpyOrder spyOrder)
        {
            if (spyOrder == null)
                return false;
            bool isDemand = false;
            foreach (var clause in spyOrder.Clauses)
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

        //public static bool HasTreaty(this ISpyOrder spyOrder)
        //{
        //    if (spyOrder == null)
        //        return false;

        //    // GameLog.Core.Diplomacy.DebugFormat("hasTreaty: null or clause = {0}", spyOrder.Clauses.ToString()/*, spyOrder.ToString()*/);

        //    foreach (var clause in spyOrder.Clauses)
        //    {
        //        switch (clause.ClauseType)
        //        {
        //            case ClauseType.TreatyDefensiveAlliance:
        //            case ClauseType.TreatyFullAlliance:
        //            case ClauseType.TreatyCeaseFire:
        //            case ClauseType.TreatyWarPact:
        //            case ClauseType.TreatyAffiliation:
        //            case ClauseType.TreatyNonAggression:
        //            case ClauseType.TreatyOpenBorders:
        //            case ClauseType.TreatyResearchPact:
        //            case ClauseType.TreatyTradePact:
        //            case ClauseType.TreatyMembership:
        //                // doesn't work
        //                //GameLog.Core.Diplomacy.DebugFormat("hasTreaty: ClauseType = {0}", clause.ClauseType.ToString()/*, spyOrder.ToString()*/);
        //                return true;
        //        }
        //    }
        //    return false;
        //}

        public static bool HasClause(this ISpyOrder spyOrder, ClauseType clause)
        {
            // GameLog.Core.Diplomacy.DebugFormat("hasClause: null or clause = {0}", clause.ToString()/*, spyOrder.ToString()*/);

            if (spyOrder == null)
                return false;

            //GameLog.Core.Diplomacy.DebugFormat("Visitor = {0}: Accepting spyOrder = {1}", attacker.ToString(), spyOrder.ToString());

            return spyOrder.Clauses.Any(c => c.ClauseType == clause);
        }

        public static void Accept([NotNull] this ISpyOrder spyOrder, [NotNull] ISpyOrderAttacker attacker)
        {
            if (spyOrder == null)
                throw new ArgumentNullException("spyOrder");
            if (attacker == null)
                throw new ArgumentNullException("attacker");

            GameLog.Core.Diplomacy.DebugFormat("Visitor = {0}: Accepting spyOrder = {1}", attacker.ToString(), spyOrder.ToString());

            //if (spyOrder.IsGift())
            //{
            //    attacker.VisitGift(spyOrder);
            //    return;
            //}

            if (spyOrder.IsDemand())   // Steal Credits ?
            {
                attacker.VisitDemand(spyOrder);
                return;
            }

            //if (spyOrder.IsWarPact())
            //{
            //    attacker.VisitWarPact(spyOrder);
            //    return;
            //}

            //if (spyOrder.HasTreaty())
            //{
            //    attacker.VisitTreatySpyOrder(spyOrder);
            //    return;
            //}

            //attacker.VisitExchange(spyOrder);
        }
    }
}