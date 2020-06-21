using System;
using System.Collections.Generic;
using System.Data.Linq;
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
        public static void DoTurn([NotNull] ICivIdentity civ) // pass in all civs to process Diplomacy
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            var Aciv = (Civilization) civ;

            var diplomat = Diplomat.Get(civ);

            /*
             * Process messages which have already been delivered
             */
            foreach (var otherCiv in GameContext.Current.Civilizations
            ) // we can control regard and trust for both human and AI otherCivs
            {
                if (otherCiv.CivID == civ.CivID)
                    continue;
                if (!DiplomacyHelper.IsContactMade(civ.CivID, otherCiv.CivID))
                    continue;

                var foreignPower = diplomat.GetForeignPower(otherCiv);

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
                int[] countArray = new int[] {foreignTraits.Length, theCivTraits.Length};
                int fewestTotalTraits = countArray.Min();

                double similarTraits = commonTraitItems.Count() / fewestTotalTraits;

                if (!Aciv.IsHuman)
                {
                    GameLog.Client.Deuterium.DebugFormat("## Beging DiplomacyAI for Aciv AI .......................");
                    // delta trust and regard by traits
                    if (foreignPower.DiplomacyData.ContactTurn > (GameContext.Current.TurnNumber - 10))
                    {
                        int lessOverTime = (GameContext.Current.TurnNumber + 1) -
                                           foreignPower.DiplomacyData.ContactTurn;
                        if (similarTraits == 1)
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent,
                                100 / lessOverTime));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner,
                                100 / lessOverTime);
                        }
                        else if (similarTraits >= 0.5)
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent,
                                50 / lessOverTime));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner,
                                50 / lessOverTime);
                        }
                        else if (similarTraits > 0.3)
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent,
                                25 / lessOverTime));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner,
                                25 / lessOverTime);
                        }
                        else if (similarTraits < 0.3)
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent,
                                -25 / lessOverTime));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner,
                                -25 / lessOverTime);
                        }

                        foreignPower.UpdateStatus();
                        GameLog.Client.Diplomacy.DebugFormat("## current Aciv ={0} otherCiv ={1} foreighPower.Counterparty ={2} foreighPower.Owner ={3}",
                            Aciv.ShortName, otherCiv.ShortName, foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);
                    }

                    if (foreignPower.ProposalReceived != null &&
                        (Civilization) civ == foreignPower.ProposalReceived.Recipient)
                    {
                        foreach (var clause in foreignPower.ProposalReceived.Clauses)
                        {
                            if (clause.ClauseType == ClauseType.OfferGiveCredits)
                            {
                                int value = (((CreditsClauseData) clause.Data).ImmediateAmount +
                                             ((CreditsClauseData) clause.Data).RecurringAmount) / 25;
                                foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent, value));
                                DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, 50);
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

                        int scaleTheOdds = (14 - (int) (similarTraits * 10) - regardFactor);
                        if (scaleTheOdds < 2)
                            scaleTheOdds = 2;
                        bool theirChance = RandomHelper.Chance(scaleTheOdds);
                        if (theirChance)
                        {
                            accept = true;
                        }

                        //decide proposal
                        if (accept)
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;
                        }
                        else
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;
                        }

                        GameLog.Client.Diplomacy.DebugFormat("## foreignPower PendingAction ={0} Counterparty = {1} Onwer = {2}",
                            foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName ,foreignPower.Owner.ShortName);

                        if (foreignPower.ProposalReceived != null)
                        {
                            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                            {
                                if (clause.ClauseType == ClauseType.TreatyMembership &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Unified ||
                                    clause.ClauseType == ClauseType.TreatyFullAlliance &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Allied ||
                                    clause.ClauseType == ClauseType.TreatyDefensiveAlliance &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Allied ||
                                    clause.ClauseType == ClauseType.TreatyWarPact &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Allied ||
                                    clause.ClauseType == ClauseType.TreatyAffiliation &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyNonAggression &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyOpenBorders &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyResearchPact &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Friend ||
                                    clause.ClauseType == ClauseType.TreatyTradePact &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Neutral ||
                                    clause.ClauseType == ClauseType.TreatyCeaseFire &&
                                    foreignPower.DiplomacyData.Regard.CurrentValue < (int) RegardValue.Neutral)
                                {
                                    foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;

                                    GameLog.Client.Diplomacy.DebugFormat("## PendingAction ={0} reset by clause - regard value, Counterparty = {1} Onwer = {2}",
                                        foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);
                                }
                            }
                        }

                        else if (foreignPower.PendingAction == PendingDiplomacyAction.AcceptProposal)
                        {
                            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                            {
                                if (clause.ClauseType == ClauseType.OfferGiveCredits)
                                {
                                    int value = (((CreditsClauseData) clause.Data).ImmediateAmount +
                                                 ((CreditsClauseData) clause.Data).RecurringAmount) / 25;
                                    foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.NoRegardEvent,
                                        value));
                                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner,
                                        50);
                                }

                                if (clause.ClauseType == ClauseType.TreatyAffiliation ||
                                    clause.ClauseType == ClauseType.TreatyCeaseFire ||
                                    clause.ClauseType == ClauseType.TreatyDefensiveAlliance ||
                                    clause.ClauseType == ClauseType.TreatyNonAggression ||
                                    clause.ClauseType == ClauseType.TreatyOpenBorders ||
                                    clause.ClauseType == ClauseType.TreatyResearchPact ||
                                    clause.ClauseType == ClauseType.TreatyTradePact ||
                                    clause.ClauseType == ClauseType.TreatyWarPact)
                                {
                                    foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyAccept,
                                        100));
                                }

                                if (clause.ClauseType == ClauseType.OfferStopPiracy ||
                                    clause.ClauseType == ClauseType.OfferWithdrawTroops ||
                                    clause.ClauseType == ClauseType.OfferEndEmbargo)
                                {
                                    foreignPower.AddRegardEvent(new RegardEvent(10,
                                        RegardEventType.BorderIncursionPullout, 100));
                                }
                                //else if (clause.ClauseType == ClauseType.OfferGiveResources)

                            }
                        }
                        foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                        foreignPower.ProposalReceived = null;
                    }
                }

                if (true) // for human and non human alike //!otherCiv.IsHuman)
                {
                    GameLog.Client.Diplomacy.DebugFormat("## Begin Statements, Human and AI civs .............................");
                    // did proposals received (incoming) now Statements outgoing
                    var otherdiplomat = Diplomat.Get(otherCiv);
                    ForeignPower otherForeignPower = otherdiplomat.GetForeignPower(civ);
                    GameLog.Client.Diplomacy.DebugFormat("## current ..................Aciv ={0} ...............otherCiv ={1}",
                            Aciv.ShortName, otherCiv.ShortName);
                    GameLog.Client.Diplomacy.DebugFormat("## otherForeignPower.Counterparty ={0} otherForeignPower.Owner ={1}",
                        otherForeignPower.Counterparty.ShortName, otherForeignPower.Owner.ShortName);
                    GameLog.Client.Diplomacy.DebugFormat("## .....foreignPower.Counterparty ={0} .....foreignPower.Owner ={1}", 
                        foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);
                    // use foreignPower for Statement and Response of AI Aciv
                    // use otherForeignPower for Statement and Response - reaction to AI 
                    if (foreignPower.StatementReceived != null)
                    {
                        GameLog.Client.Diplomacy.DebugFormat(
                            "## otherforeignPower.Statement ={0} Counterparty ={1} to {2} Regard = {3} Trust = {4}",
                            foreignPower.StatementReceived.StatementType.ToString(),
                            foreignPower.Counterparty.ShortName,
                            foreignPower.Owner.ShortName,
                            foreignPower.DiplomacyData.Regard.CurrentValue,
                            foreignPower.DiplomacyData.Trust.CurrentValue);
                        // DOING: Process statements (apply regard/trust changes, etc.)
                        if (foreignPower.StatementReceived.StatementType == StatementType.WarDeclaration)
                        {
                            GameLog.Client.Diplomacy.DebugFormat(
                                "## WarDeclaration by counterparty = {0} to {1} Regard = {2} Trust = {3}",
                                foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName,
                                foreignPower.DiplomacyData.Regard.CurrentValue,
                                foreignPower.DiplomacyData.Trust.CurrentValue);

                            //if (!foreignPower.Owner.Traits.Contains("Warlike"))
                            //{
                            //    foreignPower.AddRegardEvent(new RegardEvent(30, RegardEventType.DeclaredWar, -1000));
                            //}

                            //DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -500);

                            //if (!otherCiv.Traits.Contains("WarLike"))
                            //    otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar,
                            //        -500));
                            //DiplomacyHelper.ApplyTrustChange(otherForeignPower.Owner, otherForeignPower.Counterparty,
                            //    -200);

                            //GameLog.Client.Diplomacy.DebugFormat(
                            //    "$$$ After WarDeclaration by counterparty = {0} to {1} Regard = {2} Trust = {3}",
                            //    foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName,
                            //    foreignPower.DiplomacyData.Regard.CurrentValue,
                            //    foreignPower.DiplomacyData.Trust.CurrentValue);
                            //GameLog.Client.Diplomacy.DebugFormat(
                            //    "$$$ After WarDeclaration by counterparty their {0} Regard = {1} Trust = {2}",
                            //    foreignPower.Counterparty.ShortName,
                            //    otherForeignPower.DiplomacyData.Regard.CurrentValue,
                            //    otherForeignPower.DiplomacyData.Trust.CurrentValue);
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
                                foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.EnemySharesQuadrant,
                                    -500));
                            else
                            {
                                foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.EnemySharesQuadrant,
                                    -100));
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
                        // ToDo: look at AI civ reacting to blame with the StatementType.Sabotage... and Steal... 
                        //if (foreignPower.StatementReceived.StatementType == StatementType.SabotageOrder) // only the borg now?
                        //    foreignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.NoRegardEvent, 0));

                        // if (foreignPower.StatementReceived.StatementType == StatementType.NoStatement) // do we need something for this?

                        foreignPower.LastStatementReceived = foreignPower.StatementReceived;
                        foreignPower.StatementReceived = null;
                    }

                    if (foreignPower.ResponseReceived != null)
                    {
                        // TODO: Process responses (apply regard/trust changes, etc.)

                        if (foreignPower.ResponseReceived.ResponseType == ResponseType.Accept
                        )
                            GameLog.Client.Diplomacy.DebugFormat(
                                "## Responce type ={0} ResponseReceived by ?counterparty = {1} to {2} Regard = {3} Trust = {4}",
                                foreignPower.ResponseReceived.ResponseType.ToString(),
                                foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName,
                                foreignPower.DiplomacyData.Regard.CurrentValue,
                                foreignPower.DiplomacyData.Trust.CurrentValue);
                        // Added some positive RegardEventTypes.
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
}