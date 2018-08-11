// Clause.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Types;

namespace Supremacy.Diplomacy
{
    public interface IClause
    {
        object Data { get; set; }
        int Duration { get; set; }
        ClauseType ClauseType { get; }
        bool IsDataInitialized { get; }
    }

    internal interface IClauseInternal : IClause
    {
        void SetDataInitialized();
    }

    public static class ClauseExtensions
    {
        public static IClause Clone([NotNull] this IClause clause)
        {
            if (clause == null)
                throw new ArgumentNullException("clause");

            var clone = clause.IsDataInitialized ? new Clause(clause.ClauseType, clause.Data) : new Clause(clause.ClauseType);

            clone.Duration = clause.Duration;

            return clone;
        }

        public static T GetData<T>(this IClause clause)
        {
            if (clause.Data is T)
                return (T)clause.Data;
            return default(T);
        }

        public static bool TryGetData<T>(this IClause clause, out T data)
        {
            if (clause == null)
            {
                data = default(T);
                return false;
            }
            if (clause.Data is T)
            {
                data = (T)clause.Data;
                return true;
            }
            data = default(T);
            return false;
        }

        public static void InitializeData(this IClause clause)
        {
            if (clause.IsDataInitialized)
                return;

            var clauseInternal = clause as IClauseInternal;

            switch (clause.ClauseType)
            {
                case ClauseType.OfferGiveCredits:
                case ClauseType.RequestGiveCredits:
                    clause.Data = 0;
                    break;

                case ClauseType.OfferGiveResources:
                case ClauseType.RequestGiveResources:
                    clause.Data = new ResourceValueCollection();
                    break;

                default:
                    clause.Data = null;
                    break;
            }

            if (clauseInternal != null)
                clauseInternal.SetDataInitialized();
        }

        public static void Accept([NotNull] this IClause clause, [NotNull] IClauseVisitor visitor)
        {
            if (clause == null)
                throw new ArgumentNullException("clause");
            if (visitor == null)
                throw new ArgumentNullException("visitor");

            switch (clause.ClauseType)
            {
                case ClauseType.OfferWithdrawTroops:
                    visitor.VisitOfferWithdrawTroopsClause(clause);
                    break;
                case ClauseType.RequestWithdrawTroops:
                    visitor.VisitRequestWithdrawTroopsClause(clause);
                    break;
                case ClauseType.OfferStopPiracy:
                    visitor.VisitOfferStopPiracyClause(clause);
                    break;
                case ClauseType.RequestStopPiracy:
                    visitor.VisitRequestStopPiracyClause(clause);
                    break;
                case ClauseType.OfferBreakAgreement:
                    visitor.VisitOfferBreakAgreementClause(clause);
                    break;
                case ClauseType.RequestBreakAgreement:
                    visitor.VisitRequestBreakAgreementClause(clause);
                    break;
                case ClauseType.OfferGiveCredits:
                    visitor.VisitOfferGiveCreditsClause(clause);
                    break;
                case ClauseType.RequestGiveCredits:
                    visitor.VisitRequestGiveCreditsClause(clause);
                    break;
                case ClauseType.OfferGiveResources:
                    visitor.VisitOfferGiveResourcesClause(clause);
                    break;
                case ClauseType.RequestGiveResources:
                    visitor.VisitRequestGiveResourcesClause(clause);
                    break;
                case ClauseType.OfferMapData:
                    visitor.VisitOfferMapDataClause(clause);
                    break;
                case ClauseType.RequestMapData:
                    visitor.VisitRequestMapDataClause(clause);
                    break;
                case ClauseType.OfferHonorMilitaryAgreement:
                    visitor.VisitOfferHonorMilitaryAgreementClause(clause);
                    break;
                case ClauseType.RequestHonorMilitaryAgreement:
                    visitor.VisitRequestHonorMilitaryAgreementClause(clause);
                    break;
                case ClauseType.OfferEndEmbargo:
                    visitor.VisitOfferEndEmbargoClause(clause);
                    break;
                case ClauseType.RequestEndEmbargo:
                    visitor.VisitRequestEndEmbargoClause(clause);
                    break;
                case ClauseType.TreatyWarPact:
                    visitor.VisitWarPactClause(clause);
                    break;
                case ClauseType.TreatyCeaseFire:
                    visitor.VisitTreatyCeaseFireClause(clause);
                    break;
                case ClauseType.TreatyNonAggression:
                    visitor.VisitTreatyNonAggressionClause(clause);
                    break;
                case ClauseType.TreatyOpenBorders:
                    visitor.VisitTreatyOpenBordersClause(clause);
                    break;
                case ClauseType.TreatyTradePact:
                    visitor.VisitTreatyTradePactClause(clause);
                    break;
                case ClauseType.TreatyResearchPact:
                    visitor.VisitTreatyResearchPactClause(clause);
                    break;
                case ClauseType.TreatyAffiliation:
                    visitor.VisitTreatyAffiliationClause(clause);
                    break;
                case ClauseType.TreatyDefensiveAlliance:
                    visitor.VisitTreatyDefensiveAllianceClause(clause);
                    break;
                case ClauseType.TreatyFullAlliance:
                    visitor.VisitTreatyFullAllianceClause(clause);
                    break;
                case ClauseType.TreatyMembership:
                    visitor.VisitTreatyMembershipClause(clause);
                    break;
            }
        }
    }

    [Serializable]
    public class Clause : IClauseInternal
    {
        public const int DurationStepSize = 50;
        public const int ImmediateDuration = 1;
        public const int IndefiniteDuration = 255;
        public const int InvalidDuration = 0;
        public const int MaxFiniteDuration = 250;
        public const int NotEnded = -1;
        public const int NotStarted = -1;

        private readonly IsSetFlag _isDataInitialized;
        private int _duration = ImmediateDuration;

        public Clause() {}

        public Clause(ClauseType clauseType)
        {
            if (clauseType == ClauseType.NoClause)
                throw new ArgumentException("cannot be NoClause", "clauseType");
            _isDataInitialized = new IsSetFlag(false);
            ClauseType = clauseType;
            this.InitializeData();
        }

        public Clause(ClauseType clauseType, object data) : this(clauseType)
        {
            Data = data;
        }

        protected internal void SetDataInitialized()
        {
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            _isDataInitialized.Set();
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
        }

        #region IClauseInternal Members
        public ClauseType ClauseType { get; set; }

        public object Data { get; set; }

        public int Duration
        {
            get { return _duration; }
            set
            {
                if (value > MaxFiniteDuration)
                    value = MaxFiniteDuration;
                else if (value < ImmediateDuration)
                    value = ImmediateDuration;
                _duration = value;
            }
        }

        public bool IsDataInitialized
        {
            get { return _isDataInitialized.IsSet; }
        }

        void IClauseInternal.SetDataInitialized()
        {
            SetDataInitialized();
        }
        #endregion
    }
}