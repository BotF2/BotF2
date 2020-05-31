using System;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Utility;

namespace Supremacy.AI
{
    public static class DiplomatAI
    {
        public static void DoTurn([NotNull] ICivIdentity civ)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            var Aciv = (Civilization)civ;
            if (Aciv.IsHuman)
            {
                return;
            }
            //    return;

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
                    continue;
                
                if (foreignPower.ProposalReceived != null)
                {
                    bool accept = false;
                    #region Foriegn Traits List
                    String traitsOfForeignCiv = otherCiv.Traits;
                    var foreignTraits = traitsOfForeignCiv.Split(',');
                    #endregion

                    #region The Civ's Traits List
                    String traitsOfCiv = Aciv.Traits;
                    var theCivTraits = traitsOfCiv.Split(',');
                    #endregion

                    var commonTraitItems = foreignTraits.Intersect(theCivTraits);

                    if (commonTraitItems.Count() > 0)
                        accept = true;

                    if (accept)
                    {
                        foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;
                    }
                    else
                    {
                        foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                    }

                    GameLog.Client.Diplomacy.DebugFormat("ForeignPower = {0} Civ = {1} decsion = {2}", foreignPower.Owner.ShortName, Aciv.ShortName, foreignPower.PendingAction.ToString());
                    foreach (var clause in foreignPower.ProposalReceived.Clauses) // regard value 0 TotalWar to 5 Unified
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
                            int value = data.Sum(pair => EconomyHelper.ComputeResourceValue(pair.Item1, pair.Item2)) / 100;
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