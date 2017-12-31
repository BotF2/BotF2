// DiplomacyExtensions.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;

using System.Linq;

namespace Supremacy.Diplomacy
{
    public static class DiplomacyExtensions
    {
        public static bool IsWarPact(this IProposal proposal)
        {
            if (proposal == null)
                return false;

            return proposal.Clauses.Any(o => o.ClauseType == ClauseType.TreatyWarPact);
        }

        public static bool IncludesTreaty(this IProposal proposal)
        {
            if (proposal == null)
                return false;

            return proposal.Clauses.Any(o => o.ClauseType.IsTreatyClause());
        }

        public static bool IsTreatyClause(this ClauseType clause)
        {
            switch (clause)
            {
                case ClauseType.TreatyDefensiveAlliance:
                case ClauseType.TreatyFullAlliance:
                case ClauseType.TreatyCeaseFire:
                //case ClauseType.TreatyWarPact:
                case ClauseType.TreatyAffiliation:
                case ClauseType.TreatyNonAggression:
                case ClauseType.TreatyOpenBorders:
                case ClauseType.TreatyResearchPact:
                case ClauseType.TreatyTradePact:
                    return true;
            }
            return false;
        }

        public static bool ExcludesOnSameSide(this ClauseType clause, ClauseType otherClause)
        {
            if (clause == ClauseType.NoClause)
                return false;
            if (IsTreatyClause(clause))
                return true;
            return false;
        }

        public static bool ExcludesOnOtherSide(this ClauseType clause, ClauseType otherClause)
        {
            if (clause == ClauseType.NoClause)
                return false;
            if (IsTreatyClause(clause))
                return true;
            return false;
        }

        public static RegardEventCategories GetCategories(this RegardEventType eventType)
        {
            var categories = RegardEventCategories.None;
            switch (eventType)
            {
                case RegardEventType.LostBattle:
                    categories |= RegardEventCategories.MilitaryPower;
                    break;
                case RegardEventType.AttackedCivilians:
                    categories |= RegardEventCategories.MilitarySafety;
                    break;
                case RegardEventType.PeacetimeBorderIncursion:
                    categories |= RegardEventCategories.MilitarySafety;
                    break;
                case RegardEventType.BorderIncursionPullout:
                    categories |= RegardEventCategories.MilitarySafety;
                    break;
                case RegardEventType.InvaderMovement:
                    categories |= RegardEventCategories.MilitarySafety;
                    break;
                case RegardEventType.UnprovokedAttack:
                    categories |= RegardEventCategories.MilitarySafety;
                    break;
                case RegardEventType.ViolatedPeaceTreaty:
                    categories |= RegardEventCategories.Diplomacy;
                    break;
                case RegardEventType.ViolatedStopRaiding:
                    categories |= RegardEventCategories.Diplomacy;
                    break;
                case RegardEventType.ViolatedStopSpying:
                    categories |= RegardEventCategories.Diplomacy;
                    break;
                case RegardEventType.EnemySharesQuadrant:
                    categories |= RegardEventCategories.Diplomacy;
                    break;
                case RegardEventType.DeclaredWar:
                    // None
                    break;
                case RegardEventType.CapturedColony:
                    categories |= RegardEventCategories.MilitarySafety;
                    break;
            }
            return categories;
        }

        public static void OnAttack(this Diplomat source, Civilization aggressor)
        {
            if (source == null)
                return;
            var data = source.GetExtendedData(aggressor);
            if (data != null)
                data.OnAttack();
        }

        public static void OnIncursion(this Diplomat source, Civilization aggressor)
        {
            if (source == null)
                return;
            var data = source.GetExtendedData(aggressor);
            if (data != null)
                data.OnIncursion();
        }

        public static IProposal GetLastProposalSent(this Diplomat source, ICivIdentity civ)
        {
            if (source == null)
                return null;
            var envoy = source.GetForeignPower(civ);
            if (envoy != null)
                return envoy.LastProposalSent;
            return null;
        }

        public static IProposal GetLastProposalReceived(this Diplomat source, ICivIdentity civ)
        {
            if (source == null)
                return null;
            var envoy = source.GetForeignPower(civ);
            if (envoy != null)
                return envoy.LastProposalReceived;
            return null;
        }

        public static IResponse GetLastResponseSent(this Diplomat source, ICivIdentity civ)
        {
            if (source == null)
                return null;
            var envoy = source.GetForeignPower(civ);
            if (envoy != null)
                return envoy.LastResponseSent;
            return null;
        }

        public static IResponse GetLastResponseReceived(this Diplomat source, ICivIdentity civ)
        {
            if (source == null)
                return null;
            var envoy = source.GetForeignPower(civ);
            if (envoy != null)
                return envoy.LastResponseReceived;
            return null;
        }

        public static bool IsEmbargoInPlace(this Diplomat source, ICivIdentity civ)
        {
            if (source == null)
                return false;
            var envoy = source.GetForeignPower(civ);
            if (envoy != null)
                return envoy.IsEmbargoInPlace;
            return false;
        }

        public static bool CanCommendOrDenounceWar([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] Statement includeStatement = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            return source.GetCommendOrDenounceWarParameters(civ, includeStatement).Any();
        }

        public static IEnumerable<Civilization> GetCommendOrDenounceWarParameters([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] Statement includeStatement = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            var diplomacyDataMap = GameContext.Current.DiplomacyData;

            Civilization ignoreCiv;

            if (includeStatement != null &&
                (includeStatement.StatementType == StatementType.CommendWar ||
                 includeStatement.StatementType == StatementType.DenounceWar))
            {
                ignoreCiv = includeStatement.Parameter as Civilization;
            }
            else
            {
                ignoreCiv = null;
            }

            foreach (var otherCiv in GameContext.Current.Civilizations)
            {
                if (otherCiv.CivID == source.OwnerID || otherCiv.CivID == civ.CivID || otherCiv == ignoreCiv)
                    continue;
                
                var diplomacyData = diplomacyDataMap[civ.CivID, otherCiv.CivID];

                if (diplomacyData.Status == ForeignPowerStatus.AtWar &&
                    diplomacyData.TurnsSinceLastStatusChange <= 1)
                {
                    yield return otherCiv;
                }
            }
        }

        public static IEnumerable<Civilization> GetWarPactParameters([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            var sender = source.Owner;
            var recipient = GameContext.Current.Civilizations[civ.CivID];

            HashSet<Civilization> existingWarPacts = null;

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                            var warPactTarget = clause.Data as Civilization;
                            if (warPactTarget == null)
                                continue;

                            if (existingWarPacts == null)
                                existingWarPacts = new HashSet<Civilization>();

                            existingWarPacts.Add(warPactTarget);
                            break;
                    }
                }
            }

            foreach (var otherCiv in GameContext.Current.Civilizations)
            {
                if (otherCiv.CivID == source.OwnerID || otherCiv.CivID == civ.CivID)
                    continue;

                if (DiplomacyHelper.IsContactMade(sender, otherCiv) &&
//                    DiplomacyHelper.IsContactMade(recipient, otherCiv) &&
                    !DiplomacyHelper.AreAtWar(recipient, otherCiv) &&
                    DiplomacyHelper.IsIndependent(otherCiv) &&
                    (existingWarPacts == null || !existingWarPacts.Contains(otherCiv)))
                {
                    yield return otherCiv;
                }
            }
        }

        public static bool CanCommendOrDenounceTreaty([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] Statement includeStatement = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            return source.GetCommendOrDenounceTreatyParameters(civ, includeStatement).Any();
        }

        public static IEnumerable<Civilization> GetCommendOrDenounceTreatyParameters([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] Statement includeStatement = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            var currentTurn = GameContext.Current.TurnNumber;
            var agreementMatrix = GameContext.Current.AgreementMatrix;

            Civilization ignoreCiv;

            if (includeStatement != null &&
                (includeStatement.StatementType == StatementType.CommendWar ||
                 includeStatement.StatementType == StatementType.DenounceWar))
            {
                ignoreCiv = includeStatement.Parameter as Civilization;
            }
            else
            {
                ignoreCiv = null;
            }

            foreach (var otherCiv in GameContext.Current.Civilizations)
            {
                if (otherCiv.CivID == source.OwnerID || otherCiv.CivID == civ.CivID || otherCiv == ignoreCiv)
                    continue;

                if (!source.GetForeignPower(otherCiv).IsContactMade)
                    continue;

                var treatyAgreement = agreementMatrix.FindAgreement(
                    civ,
                    otherCiv,
                    agreement => agreement.Proposal.IncludesTreaty() &&
                                 currentTurn - agreement.StartTurn <= 1);

                if (treatyAgreement != null)
                    yield return otherCiv;
            }
        }

        public static IEnumerable<Civilization> GetRequestHonorMilitaryAgreementParameters([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            if (!agreementMatrix.IsAgreementActive(source.OwnerID, civ.CivID, ClauseType.TreatyDefensiveAlliance) &&
                !agreementMatrix.IsAgreementActive(source.OwnerID, civ.CivID, ClauseType.TreatyFullAlliance))
            {
                yield break;
            }

            var atWarWith = new HashSet<Civilization>();
            var civilizations = GameContext.Current.Civilizations;
            var counterparty = civilizations[civ.CivID];

            atWarWith.UnionWith(
                from d in GameContext.Current.DiplomacyData.GetValuesForOwner(source.Owner)
                where d.Status == ForeignPowerStatus.AtWar && d.CounterpartyID != civ.CivID
                select civilizations[d.CounterpartyID]);

            atWarWith.ExceptWith(
                from d in GameContext.Current.DiplomacyData.GetValuesForOwner(counterparty)
                where d.Status == ForeignPowerStatus.AtWar && d.CounterpartyID != source.OwnerID
                select civilizations[d.CounterpartyID]);

            if (includeProposal != null)
            {
                foreach (var clauses in includeProposal.Clauses.Where(o => o.ClauseType == ClauseType.RequestHonorMilitaryAgreement))
                {
                    var otherCiv = clauses.Data as Civilization;
                    if (otherCiv != null)
                        atWarWith.Remove(otherCiv);
                }
            }

            foreach (var civilization in atWarWith)
                yield return civilization;
        }

        public static IEnumerable<Civilization> GetOfferHonorMilitaryAgreementParameters([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            if (!agreementMatrix.IsAgreementActive(source.OwnerID, civ.CivID, ClauseType.TreatyDefensiveAlliance) &&
                !agreementMatrix.IsAgreementActive(source.OwnerID, civ.CivID, ClauseType.TreatyFullAlliance))
            {
                yield break;
            }

            var atWarWith = new HashSet<Civilization>();
            var civilizations = GameContext.Current.Civilizations;
            var counterparty = civilizations[civ.CivID];

            atWarWith.UnionWith(
                from d in GameContext.Current.DiplomacyData.GetValuesForOwner(counterparty)
                where d.Status == ForeignPowerStatus.AtWar && d.CounterpartyID != source.OwnerID
                select civilizations[d.CounterpartyID]);

            atWarWith.ExceptWith(
                from d in GameContext.Current.DiplomacyData.GetValuesForOwner(source.Owner)
                where d.Status == ForeignPowerStatus.AtWar && d.CounterpartyID != civ.CivID
                select civilizations[d.CounterpartyID]);

            if (includeProposal != null)
            {
                foreach (var clauses in includeProposal.Clauses.Where(o => o.ClauseType ==ClauseType.OfferHonorMilitaryAgreement))
                {
                    var otherCiv = clauses.Data as Civilization;
                    if (otherCiv != null)
                        atWarWith.Remove(otherCiv);
                }
            }

            foreach (var civilization in atWarWith)
                yield return civilization;
        }

        public static bool CanProposeWarPact([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyCeaseFire:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            if (DiplomacyHelper.IsMember(GameContext.Current.Civilizations[civ.CivID], source.Owner))
                return false;

            return GetWarPactParameters(source, civ, includeProposal).Any();
        }

        public static bool CanProposeCeaseFire([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyCeaseFire:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]);
        }

        public static bool CanProposeNonAggressionTreaty([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            var activeAgreements = agreementMatrix[source.OwnerID, civ.CivID];

            foreach (var agreement in activeAgreements)
            {
                foreach (var clause in agreement.Proposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool CanProposeOpenBordersTreaty([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            var activeAgreements = agreementMatrix[source.OwnerID, civ.CivID];

            foreach (var agreement in activeAgreements)
            {
                foreach (var clause in agreement.Proposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool CanProposeAffiliation([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (!source.Owner.IsEmpire || !GameContext.Current.Civilizations[civ.CivID].IsEmpire)
                return false;

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyCeaseFire:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            var activeAgreements = agreementMatrix[source.OwnerID, civ.CivID];

            foreach (var agreement in activeAgreements)
            {
                foreach (var clause in agreement.Proposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool CanProposeDefensiveAlliance([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (!source.Owner.IsEmpire || !GameContext.Current.Civilizations[civ.CivID].IsEmpire)
                return false;

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyCeaseFire:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            var activeAgreements = agreementMatrix[source.OwnerID, civ.CivID];

            foreach (var agreement in activeAgreements)
            {
                foreach (var clause in agreement.Proposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool CanProposeFullAlliance([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (!source.Owner.IsEmpire || !GameContext.Current.Civilizations[civ.CivID].IsEmpire)
                return false;

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyCeaseFire:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            var activeAgreements = agreementMatrix[source.OwnerID, civ.CivID];

            foreach (var agreement in activeAgreements)
            {
                foreach (var clause in agreement.Proposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return true;
        }

        public static bool CanProposeMembership([NotNull] this Diplomat source, [NotNull] ICivIdentity civ, [CanBeNull] IProposal includeProposal = null)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (civ == null)
                throw new ArgumentNullException("civ");

            // If both parties are empires, or both parties are minor races, then membership can't be proposed.
            if (source.Owner.IsEmpire == GameContext.Current.Civilizations[civ.CivID].IsEmpire)
                return false;

            if (includeProposal != null)
            {
                foreach (var clause in includeProposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyWarPact:
                        case ClauseType.TreatyCeaseFire:
                        case ClauseType.TreatyNonAggression:
                        case ClauseType.TreatyOpenBorders:
                        case ClauseType.TreatyAffiliation:
                        case ClauseType.TreatyDefensiveAlliance:
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            if (DiplomacyHelper.AreAtWar(source.Owner, GameContext.Current.Civilizations[civ.CivID]))
                return false;

            var agreementMatrix = GameContext.Current.AgreementMatrix;
            var activeAgreements = agreementMatrix[source.OwnerID, civ.CivID];

            foreach (var agreement in activeAgreements)
            {
                foreach (var clause in agreement.Proposal.Clauses)
                {
                    switch (clause.ClauseType)
                    {
                        case ClauseType.TreatyFullAlliance:
                        case ClauseType.TreatyMembership:
                            return false;
                    }
                }
            }

            return true;
        }
    }
}