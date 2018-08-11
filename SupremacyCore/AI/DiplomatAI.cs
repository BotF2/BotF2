using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.AI
{
    public static class DiplomatAI
    {
        public static void DoTurn([NotNull] ICivIdentity civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            if (PlayerContext.Current.IsHumanPlayer(civ))
                return;

            var diplomat = Diplomat.Get(civ);

            /*
             * Process messages which have already been delivered
             */
            foreach (var otherCiv in GameContext.Current.Civilizations)
            {
                if (otherCiv.CivID == civ.CivID)
                    continue;

                var foreignPower = diplomat.GetForeignPower(otherCiv);
                if (!foreignPower.IsContactMade)
                    return;
                
                if (foreignPower.ProposalReceived != null)
                {
                    // TODO: Have the AI actually consider proposals instead of blindly accepting
                    foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;
                    foreach (var clause in foreignPower.ProposalReceived.Clauses)
                    {
                        if (clause.ClauseType == ClauseType.TreatyMembership && foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Unified ||
                            clause.ClauseType == ClauseType.TreatyFullAlliance && foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Allied ||
                            clause.ClauseType == ClauseType.TreatyDefensiveAlliance && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Allied ||
                            clause.ClauseType == ClauseType.TreatyWarPact && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Allied ||
                            clause.ClauseType == ClauseType.TreatyAffiliation && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend || 
                            clause.ClauseType == ClauseType.TreatyNonAggression && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                            clause.ClauseType == ClauseType.TreatyOpenBorders && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                            clause.ClauseType == ClauseType.TreatyResearchPact && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                            clause.ClauseType == ClauseType.TreatyTradePact && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Neutral ||
                            clause.ClauseType == ClauseType.TreatyCeaseFire && foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Neutral)
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                        }
                    }

                    if (foreignPower.PendingAction == PendingDiplomacyAction.AcceptProposal)
                    {
                        foreach (var clause in foreignPower.ProposalReceived.Clauses)
                        {
                            if (clause.ClauseType == ClauseType.OfferGiveCredits)
                            {
                                int value = (((CreditsClauseData)clause.Data).ImmediateAmount + ((CreditsClauseData)clause.Data).RecurringAmount) / 100;
                                foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, value));
                            }
                            else if (clause.ClauseType == ClauseType.OfferGiveResources)
                            {
                                var data = (IEnumerable<Tuple<ResourceType, int>>)clause.Data;
                                int value = data.Sum(pair => DiplomacyHelper.ComputeResourceValue(pair.Item1, pair.Item2)) / 100;
                                foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, value));
                            }
                        }
                    }

                    foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                    foreignPower.ProposalReceived = null;
                }

                if (foreignPower.StatementReceived != null)
                {
                    // TODO: Process statements (apply regard/trust changes, etc.)
                    if (foreignPower.StatementReceived.StatementType == StatementType.WarDeclaration)
                        foreignPower.AddRegardEvent(new RegardEvent(30, RegardEventType.DeclaredWar, -1000));
                    
                    foreignPower.LastStatementReceived = foreignPower.StatementReceived;
                    foreignPower.StatementReceived = null;
                }

                if (foreignPower.ResponseReceived != null)
                {
                    // TODO: Process responses (apply regard/trust changes, etc.)
                    foreignPower.LastResponseReceived = foreignPower.ResponseReceived;
                    foreignPower.ResponseReceived = null;
                }
                
                foreignPower.UpdateRegardAndTrustMeters();
                foreignPower.UpdateStatus();
            }
        }
    }
}