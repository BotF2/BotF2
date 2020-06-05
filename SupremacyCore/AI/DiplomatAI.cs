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
    public delegate bool Chance();
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
                    string traitsOfForeignCiv = otherCiv.Traits;
                    string[] foreignTraits = traitsOfForeignCiv.Split(',');
                    #endregion

                    #region The Civ's Traits List
                    string traitsOfCiv = Aciv.Traits;
                    string[] theCivTraits = traitsOfCiv.Split(',');
                    #endregion

                    // traits in common relative to the number of triats a civilization has
                    var commonTraitItems = foreignTraits.Intersect(theCivTraits);
                    int[] countArray = new int[] { foreignTraits.Length, theCivTraits.Length };
                    int fewestTotalTraits = countArray.Min();

                    double similarTraits = commonTraitItems.Count() / fewestTotalTraits;

                    if ( similarTraits == 1 && RandomHelper.Chance(2))
                    {
                        accept = true;
                    }
                    else if (similarTraits > 0.6 && RandomHelper.Chance(3))
                    {
                        accept = true;
                    }
                    else if (similarTraits >= 0.5 && RandomHelper.Chance(4))
                    {
                        accept = true;
                    }
                    else if (similarTraits > 0.3 && RandomHelper.Chance(6))
                    {
                        accept = true;
                    }
                    else if (similarTraits == 0 && RandomHelper.Chance(12))
                    {
                        accept = true;
                    }

                    if (accept)
                    {
                        foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;
                        GameLog.Client.Diplomacy.DebugFormat("Accept Proposal {1} and {2} - CommonTraits = {0}", commonTraitItems.Count(), foreignPower.Owner.ShortName, Aciv.ShortName);
                    }
                    else
                    {
                        foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                        GameLog.Client.Diplomacy.DebugFormat("Reject Proposal {1} and {2} - CommonTraits = {0}", commonTraitItems.Count(), foreignPower.Owner.ShortName, Aciv.ShortName);
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
                    {
                        GameLog.Client.Diplomacy.DebugFormat("$$$ WarDeclaration Statement Owner = {0} to {1} for RegardEvent DeclareWar",
                            foreignPower.Owner.ShortName, foreignPower.Counterparty.ShortName);

                        foreignPower.AddRegardEvent(new RegardEvent(30, RegardEventType.DeclaredWar, -1000));

                        GameLog.Client.Diplomacy.DebugFormat("$$$ CounterparytID  ={0}, Owner regard ={1}, Counterparty regard ={2} ",
                            foreignPower.DiplomacyData.CounterpartyID, foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue);
                    }
                    if (foreignPower.StatementReceived.StatementType == StatementType.ThreatenTradeEmbargo
                        || foreignPower.StatementReceived.StatementType == StatementType.ThreatenDestroyColony
                        || foreignPower.StatementReceived.StatementType == StatementType.ThreatenDeclareWar)
                        foreignPower.AddRegardEvent(new RegardEvent(20, RegardEventType.InvaderMovement, -600));
                    if (foreignPower.StatementReceived.StatementType == StatementType.DenounceWar
                        || foreignPower.StatementReceived.StatementType == StatementType.DenounceSabotage
                        || foreignPower.StatementReceived.StatementType == StatementType.DenounceInvasion
                        || foreignPower.StatementReceived.StatementType == StatementType.DenounceAssault)
                        foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, +400));
                    if (foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                        || foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                        || foreignPower.StatementReceived.StatementType == StatementType.CommendRelationship
                        || foreignPower.StatementReceived.StatementType == StatementType.CommendSabotage
                        || foreignPower.StatementReceived.StatementType == StatementType.CommendWar)
                        foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, +400));
                    if (foreignPower.StatementReceived.StatementType == StatementType.SabotageOrder) // only the borg now?
                        foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.AttackedCivilians, -400));
                    // if (foreignPower.StatementReceived.StatementType == StatementType.NoStatement) // do we need something for this?
                    foreignPower.LastStatementReceived = foreignPower.StatementReceived;
                    foreignPower.StatementReceived = null;
                }

                if (foreignPower.ResponseReceived != null)
                {
                    // TODO: Process responses (apply regard/trust changes, etc.)
                    
                    //if (foreignPower.ResponseReceived.ResponseType == ResponseType.Reject) // The ResponseTypes do not appear to have matching regard events at this time.
                    //    foreignPower.AddRegardEvent(new RegardEvent(30, RegardEventType., -1000));
                    foreignPower.LastResponseReceived = foreignPower.ResponseReceived;
                    foreignPower.ResponseReceived = null;

                }
                
                foreignPower.UpdateRegardAndTrustMeters();
                foreignPower.UpdateStatus();
            }
        }
    }
}