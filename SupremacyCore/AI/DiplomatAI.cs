using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata;
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
        public static void DoTurn([NotNull] ICivIdentity civ) // pass in the AI players to procees Diplomacy
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
            foreach (var otherCiv in GameContext.Current.Civilizations) // we can control regard and trust for both human and AI otherCivs
            {
                if (otherCiv.CivID == civ.CivID)

                    continue;
                var foreignPower = diplomat.GetForeignPower(otherCiv);
                if (!foreignPower.IsContactMade)
                    continue;
 
                if (foreignPower.ProposalReceived != null && (Civilization)civ == foreignPower.ProposalReceived.Recipient)
                 {
                     foreach (var clause in foreignPower.ProposalReceived.Clauses)
                     {
                         if (clause.ClauseType == ClauseType.OfferGiveCredits)
                         {
                             int value = (((CreditsClauseData)clause.Data).ImmediateAmount + ((CreditsClauseData)clause.Data).RecurringAmount) / 25;
                             foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, value));
                         }
                     }
                    int regardFactor = 0;
                    switch (foreignPower.DiplomacyData.EffectiveRegard)
                    {
                        case RegardLevel.TotalWar:
                            regardFactor = -100;
                            break;
                        case RegardLevel.ColdWar:
                            regardFactor = -50;
                            break;
                        case RegardLevel.Detested:
                            regardFactor = -25;
                            break;
                        case RegardLevel.Neutral:
                            regardFactor = 0;
                            break;
                        case RegardLevel.Friend:
                            regardFactor = 4;
                            break;
                        case RegardLevel.Unified:
                            regardFactor = 8;
                            break;
                        case RegardLevel.Allied:
                            regardFactor = 12;
                            break;
                    }
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
                    int scaleTheOdds = (14 -(int)(similarTraits * 10) - regardFactor);
                    if (scaleTheOdds < 2)
                        scaleTheOdds = 2;
                    bool theirChance = RandomHelper.Chance(scaleTheOdds);
                    if (theirChance)
                    {
                        accept = true;
                    }

                    if (!otherCiv.IsHuman)
                    {
                        if (accept)
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;
                            GameLog.Client.Diplomacy.DebugFormat("Accept Proposal {1} and {2} - CommonTraits = {0}",
                                commonTraitItems.Count(), foreignPower.Owner.ShortName, Aciv.ShortName);
                        }
                        else
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                            GameLog.Client.Diplomacy.DebugFormat("Reject Proposal {1} and {2} - CommonTraits = {0}",
                                commonTraitItems.Count(), foreignPower.Owner.ShortName, Aciv.ShortName);
                        }

                        GameLog.Client.Diplomacy.DebugFormat("ForeignPower = {0} Civ = {1} decsion = {2}", foreignPower.Owner.ShortName, Aciv.ShortName, foreignPower.PendingAction.ToString());
                        if (foreignPower.ProposalReceived != null)
                        {
                            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                            {
                                if (clause.ClauseType == ClauseType.TreatyMembership &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Unified ||
                                    clause.ClauseType == ClauseType.TreatyFullAlliance &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Allied ||
                                    clause.ClauseType == ClauseType.TreatyDefensiveAlliance &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Allied ||
                                    clause.ClauseType == ClauseType.TreatyWarPact &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Allied ||
                                    clause.ClauseType == ClauseType.TreatyAffiliation &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyNonAggression &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyOpenBorders &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyResearchPact &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyTradePact &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Neutral ||
                                    clause.ClauseType == ClauseType.TreatyCeaseFire &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int)RegardValue.Neutral)
                                {
                                    foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                                }
                            }
                        }
                    }

                    if (foreignPower.PendingAction == PendingDiplomacyAction.AcceptProposal)
                    {
                        foreach (var clause in foreignPower.ProposalReceived.Clauses)
                        {
                            if (clause.ClauseType == ClauseType.OfferGiveCredits)
                            {
                                int value = (((CreditsClauseData)clause.Data).ImmediateAmount + ((CreditsClauseData)clause.Data).RecurringAmount) / 25;
                                foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, value));
                            }
                            //else if (clause.ClauseType == ClauseType.OfferGiveResources)
                            //{
                            //    var data = (IEnumerable<Tuple<ResourceType, int>>)clause.Data;
                            //    int value = data.Sum(pair => EconomyHelper.ComputeResourceValue(pair.Item1, pair.Item2)) / 100;
                            //    foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, value));
                            //}
                        }
                    }

                    foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                    foreignPower.ProposalReceived = null;
                }

                var otherdiplomat = Diplomat.Get(otherCiv);
                ForeignPower otherForeignPower = otherdiplomat.GetForeignPower(civ);
                GameLog.Client.Diplomacy.DebugFormat("#### Civ = {0}, ForeignPower to {1}, otherCiv = {2}, otherForeignpower to {3}", (Civilization)civ, otherCiv.ShortName, otherCiv.ShortName, (Civilization)civ);
                GameLog.Client.Diplomacy.DebugFormat("#### Foreign{ower for = {0}, to = {1} otherForeignPower for {2} to {3}", foreignPower.Owner.ShortName, foreignPower.Counterparty.ShortName, otherForeignPower.Owner.ShortName, otherForeignPower.Counterparty.ShortName);

                if (foreignPower.StatementReceived != null)
                {
                    // DOING: Process statements (apply regard/trust changes, etc.)
                    if (foreignPower.StatementReceived.StatementType == StatementType.WarDeclaration)
                    {
                        GameLog.Client.Diplomacy.DebugFormat("$$$ Before WarDeclaration by counterparty = {0} to {1} Regard = {2} Trust = {3}", 
                            foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName, foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.DiplomacyData.Trust.CurrentValue);

                        if (!foreignPower.Owner.Traits.Contains("Warlike"))
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(30, RegardEventType.DeclaredWar, -1000));
                        }
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -500);

                        if(!otherCiv.Traits.Contains("WarLike"))
                            otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, -500));
                        DiplomacyHelper.ApplyTrustChange(otherForeignPower.Owner, otherForeignPower.Counterparty, -200);

                        GameLog.Client.Diplomacy.DebugFormat("$$$ After WarDeclaration by counterparty = {0} to {1} Regard = {2} Trust = {3}",
                            foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName, foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.DiplomacyData.Trust.CurrentValue);
                        GameLog.Client.Diplomacy.DebugFormat("$$$ After WarDeclaration by counterparty their {0} Regard = {1} Trust = {2}",
                            foreignPower.Counterparty.ShortName, otherForeignPower.DiplomacyData.Regard.CurrentValue, otherForeignPower.DiplomacyData.Trust.CurrentValue);
                    }

                    //if (foreignPower.StatementReceived.StatementType == StatementType.ThreatenTradeEmbargo
                    //    || foreignPower.StatementReceived.StatementType == StatementType.ThreatenDestroyColony
                    //    || foreignPower.StatementReceived.StatementType == StatementType.ThreatenDeclareWar)
                    //{
                    //    foreignPower.AddRegardEvent(new RegardEvent(20, RegardEventType.PeacetimeBorderIncursion, -500));
                    //    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -300);
                    //    CounterpartyforeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, -200));
                    //    DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, -100);
                    //}

                    if (foreignPower.StatementReceived.StatementType == StatementType.DenounceWar)
                        //|| foreignPower.StatementReceived.StatementType == StatementType.DenounceSabotage
                        //|| foreignPower.StatementReceived.StatementType == StatementType.DenounceInvasion
                        //|| foreignPower.StatementReceived.StatementType == StatementType.DenounceAssault)
                    {
                        if (!foreignPower.Owner.Traits.Contains("Warlike"))
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.EnemySharesQuadrant, -500));
                        else
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.EnemySharesQuadrant, -100));
                        }
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -200);
                        otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, -50));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, -50);
                    }

                    if (foreignPower.StatementReceived.StatementType == StatementType.CommendWar)
                        //||foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                        //|| foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                        //|| foreignPower.StatementReceived.StatementType == StatementType.CommendRelationship
                        //|| foreignPower.StatementReceived.StatementType == StatementType.CommendSabotage)
                    {
                        if (foreignPower.Owner.Traits.Contains("Warlike"))
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, +500));
                        else if (foreignPower.Owner.Traits.Contains("Peaceful"))
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, -100));
                        }
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +200);
                        otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, +50));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, +100);
                    }

                    if (foreignPower.StatementReceived.StatementType == StatementType.SabotageOrder) // only the borg now?
                        foreignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.NoRegardEvent, 0));
                    // if (foreignPower.StatementReceived.StatementType == StatementType.NoStatement) // do we need something for this?

                    foreignPower.LastStatementReceived = foreignPower.StatementReceived;
                    foreignPower.StatementReceived = null;
                }

                if (foreignPower.ResponseReceived != null)
                {
                    // TODO: Process responses (apply regard/trust changes, etc.)

                    if (foreignPower.ResponseReceived.ResponseType == ResponseType.Accept) // Added some positive RegardEventTypes.
                    {
                        foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyAccept, +200));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +200);
                        otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyAccept, +200));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, +200);
                    }
                    if (foreignPower.ResponseReceived.ResponseType == ResponseType.Reject) 
                    {
                        foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyReject, 0));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -100);
                        otherForeignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.TreatyReject, -0));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, -0);
                    }
                    if (foreignPower.ResponseReceived.ResponseType == ResponseType.Counter) 
                    {
                        foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +100));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +50);
                        otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +50));
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, +50);
                    }
                    if (foreignPower.ResponseReceived.ResponseType == ResponseType.NoResponse) // do we need this?
                    {
                        //foreignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.BorderIncursionPullout, +0));
                        //DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +0);
                    }

                    foreignPower.LastResponseReceived = foreignPower.ResponseReceived;
                    foreignPower.ResponseReceived = null;

                }
                
                foreignPower.UpdateRegardAndTrustMeters();
                foreignPower.UpdateStatus();
            }
        }
    }
}