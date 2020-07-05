using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Runtime.Remoting.Metadata;
using System.Windows.Input;
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
        public static void DoTurn([NotNull] ICivIdentity civ) // pass in all civs to process Diplomacy
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            var aCiv = (Civilization) civ;

            var diplomat = Diplomat.Get(civ);

            /*
             * Process messages which have already been delivered
             */
            foreach (var otherCiv in GameContext.Current.Civilizations)
             // we can control regard and trust for both human otherCivs and AI otherCivs
            {
                if (otherCiv.CivID == civ.CivID)
                    continue;
                if (!DiplomacyHelper.IsContactMade(civ.CivID, otherCiv.CivID))
                    continue;
                if (!otherCiv.IsEmpire && !aCiv.IsEmpire)
                    continue;
                var foreignPower = diplomat.GetForeignPower(otherCiv);
                var otherdiplomat = Diplomat.Get(otherCiv);
                ForeignPower otherForeignPower = otherdiplomat.GetForeignPower(civ);
                if (foreignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember || otherForeignPower.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
                    continue;

                #region Foriegn Traits List

                string traitsOfForeignCiv = otherCiv.Traits;
                string[] foreignTraits = traitsOfForeignCiv.Split(',');

                #endregion

                #region The Civ's Traits List

                string traitsOfCiv = aCiv.Traits;
                string[] theCivTraits = traitsOfCiv.Split(',');

                #endregion

                // traits in common relative to the number of triats a civilization has
                var commonTraitItems = foreignTraits.Intersect(theCivTraits);

                int countCommon = 0;
                foreach (string aString in commonTraitItems)
                {
                    countCommon++;
                }

                int[] countArray = new int[] {foreignTraits.Length, theCivTraits.Length};
                int fewestTotalTraits = countArray.Min();    

                int similarTraits = (countCommon *10 / fewestTotalTraits); // (a double from 1 to 0) * 10 
                GameLog.Client.Diplomacy.DebugFormat("## similar traits ={0} counterparty ={1} traits ={2} owner ={3} traits ={4}",
                    similarTraits, foreignPower.Counterparty.Key,foreignPower.Counterparty.Traits, foreignPower.Owner.Key, foreignPower.Owner.Traits );

                /*
                 * look for human to human proposals
                 */
                if (aCiv.IsHuman && otherCiv.IsHuman)
                {
                    GameLog.Client.Diplomacy.DebugFormat("!! HUMAN counterparty {0} to HUMAN owner {1}...",
                        foreignPower.Counterparty.Key, foreignPower.Owner.Key);
                    if (foreignPower.ProposalReceived != null)
                    {
                        if (aCiv == foreignPower.ProposalReceived.Recipient)
                        {
                            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                            {

                                GameLog.Client.Diplomacy.DebugFormat("%% Clause {0} duration {1}",
                                        clause.ClauseType.ToString(), clause.Duration);                                      
                            }
                        }
                    }
                }

                if (true)//(!aCiv.IsHuman)
                {
                    GameLog.Client.Diplomacy.DebugFormat("## Beging DiplomacyAI for aCiv AI .......................");
                    #region First Impression
                    // First impression delta trust and regard by traits
                    if (!foreignPower.DiplomacyData.FirstDiplomaticAction)
                    {
                        foreignPower.DiplomacyData.FirstDiplomaticAction = true;
                        int impact = 75;
                        TrustAndRegardByTraits(foreignPower, impact, similarTraits);
                        TrustAndRegardByTraits(otherForeignPower, impact, similarTraits);
                        GameLog.Client.Diplomacy.DebugFormat("## foreignPower CounterParty ={0} regard ={1} trust ={2}",foreignPower.Counterparty.Key,foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue, foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue);
                        GameLog.Client.Diplomacy.DebugFormat("## foreignPower .......Owner ={0} regard ={1} trust ={2}", foreignPower.Owner.Key, foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.DiplomacyData.Trust.CurrentValue);
                        //foreignPower.UpdateStatus();
                        GameLog.Client.Diplomacy.DebugFormat("## current aCiv ={0} otherCiv ={1} foreighPower.Counterparty ={2} foreighPower.Owner ={3}",
                            aCiv.ShortName, otherCiv.ShortName, foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);
                        GameLog.Client.Diplomacy.DebugFormat("## Counterparty Status {0} Owner Status {1}",
                            foreignPower.CounterpartyDiplomacyData.Status.ToString(),
                            foreignPower.DiplomacyData.Status.ToString());
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty Regard={0} Trust={1} Owner Regard={2} Trust={3}",
                        //    foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue,
                        //    foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue,
                        //    foreignPower.DiplomacyData.Regard.CurrentValue,
                        //    foreignPower.DiplomacyData.Trust.CurrentValue);
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty effective regard ={0} ", foreignPower.CounterpartyDiplomacyData.EffectiveRegard.ToString());
                    }
                    #endregion First Impressions

                    #region Propsals to AI aCiv
                    /*
                      proposals
                    */
                    if (foreignPower.ProposalReceived != null)
                    {
                        if (aCiv == foreignPower.ProposalReceived.Recipient)
                        {
                            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                            {
                                if (clause.ClauseType == ClauseType.OfferGiveCredits)
                                {
                                    int value = (((CreditsClauseData)clause.Data).ImmediateAmount +
                                                 ((CreditsClauseData)clause.Data).RecurringAmount) / 25;
                                    int greedy = 0;
                                    if (foreignPower.ProposalReceived.Recipient.Traits.Contains("Materialistic"))
                                    {
                                        greedy = 50;
                                    }
                                    foreignPower.AddRegardEvent(
                                        new RegardEvent(5, RegardEventType.NoRegardEvent, value / 2 + greedy));
                                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner,
                                        value / 2 + greedy);
                                }
                                if (clause.ClauseType == ClauseType.RequestGiveCredits)
                                {
                                    foreignPower.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent, -100));
                                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -100);
                                }
                            }
                        }

                        GameLog.Client.Diplomacy.DebugFormat(
                            "## foreignPower PendingAction ={0} Counterparty = {1} Onwer = {2}",
                            foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                            foreignPower.Owner.ShortName);
                        //if (foreignPower.DiplomacyData.Status == ForeignPowerStatus.Affiliated)

                        if (foreignPower.ProposalReceived != null && !aCiv.IsHuman) //!(aCiv.IsHuman && otherCiv.IsHuman))
                        {
                            bool accepted = false;
                            int regard = foreignPower.DiplomacyData.Regard.CurrentValue;
                            int trust = foreignPower.DiplomacyData.Regard.CurrentValue;
                            //bool traits = RandomHelper.Chance(similarTraits);

                            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                            {
                                switch (clause.ClauseType)
                                {
                                    case ClauseType.TreatyMembership:
                                        if (regard > 899 && trust > 899) accepted = true; break;

                                    case ClauseType.TreatyFullAlliance:
                                        if (regard > 899 && trust > 899) accepted = true; break;

                                    case ClauseType.TreatyDefensiveAlliance:
                                        if (regard > 799 && trust > 799) accepted = true; break;

                                    case ClauseType.TreatyWarPact:
                                        if (regard > 799 && trust > 799) accepted = true; break;

                                    case ClauseType.TreatyAffiliation:
                                        if (regard > 699 && trust > 699) accepted = true; break;

                                    case ClauseType.TreatyNonAggression:
                                        if (regard > 499 && trust > 499) accepted = true; break;

                                    case ClauseType.TreatyOpenBorders:
                                        if (regard > 399 && trust > 399) accepted = true; break;

                                    case ClauseType.TreatyCeaseFire:
                                        {
                                            Random num = new Random();
                                            int chance = num.Next(1, (similarTraits + 2));
                                            if (chance != 1) accepted = true; break;
                                        }
                                    case ClauseType.OfferGiveCredits:
                                        accepted = true;
                                        break;
                                    //case ClauseType.TreatyWarPact
                                    case ClauseType.RequestGiveCredits:
                                        break;

                                    default:
                                        break;
                                }
                            }


                            if (accepted == true)
                            {
                                foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;

                                GameLog.Client.Diplomacy.DebugFormat(
                                    "## PendingAction: ACCEPT ={0} reset by clause - regard value, Counterparty = {1} Onwer = {2}",
                                    foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                                    foreignPower.Owner.ShortName);
                            }
                            else
                            {
                                foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;

                                GameLog.Client.Diplomacy.DebugFormat(
                                    "## PendingAction: REJECT ={0} reset by clause - regard value, Counterparty = {1} Onwer = {2}",
                                    foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                                    foreignPower.Owner.ShortName);
                            }


                        }
                        #endregion Proposals
                        foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                        foreignPower.ProposalReceived = null;
                    }
                    
                }

                if (true) // for human and non human alike )
                {
                    GameLog.Client.Diplomacy.DebugFormat("## Begin Statements, Human and AI civs .............................");
                    // did proposals received (incoming) now Statements outgoing
                    //var otherdiplomat = Diplomat.Get(otherCiv);
                    //ForeignPower otherForeignPower = otherdiplomat.GetForeignPower(civ);
                    GameLog.Client.Diplomacy.DebugFormat("## current ..................aCiv ={0} ...............otherCiv ={1}",
                            aCiv.ShortName, otherCiv.ShortName);
                    GameLog.Client.Diplomacy.DebugFormat("## otherForeignPower.Counterparty ={0} otherForeignPower.Owner ={1}",
                        otherForeignPower.Counterparty.ShortName, otherForeignPower.Owner.ShortName);
                    GameLog.Client.Diplomacy.DebugFormat("## .....foreignPower.Counterparty ={0} .....foreignPower.Owner ={1}", 
                        foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);


                    // use foreignPower for Statement and Response of AI aCiv
                    // use otherForeignPower for Statement and Response - reaction to AI 
                    #region Statments
                    // statements ToDo: where do we make statements?
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
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, -300));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -310);
                            int impact = -175;
                            TrustAndRegardByTraits(foreignPower, impact, similarTraits);
                            int degree = 0;
                            TrustAndRegardForATrait(foreignPower, degree, foreignTraits, theCivTraits);


                            GameLog.Client.Diplomacy.DebugFormat(
                                    "## WarDeclaration by counterparty = {0} to {1} Regard = {2} Trust = {3}",
                                    foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName,
                                    foreignPower.DiplomacyData.Regard.CurrentValue,
                                    foreignPower.DiplomacyData.Trust.CurrentValue);
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
                            //TrustAndRegardByTraits(similarTraits, foreignPower, impact);
                        }

                        if (foreignPower.StatementReceived.StatementType == StatementType.CommendWar)
                            //||foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                            //|| foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                            //|| foreignPower.StatementReceived.StatementType == StatementType.CommendRelationship
                            //|| foreignPower.StatementReceived.StatementType == StatementType.CommendSabotage)
                        {
                            //TrustAndRegardByTraits(similarTraits, foreignPower, impact);
                        }
                        // ToDo: look at AI civ reacting to blame with the StatementType.Sabotage... and Steal... 
                        //if (foreignPower.StatementReceived.StatementType == StatementType.SabotageOrder) // only the borg now?
                        //    foreignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.NoRegardEvent, 0));

                        // if (foreignPower.StatementReceived.StatementType == StatementType.NoStatement) // do we need something for this?

                        foreignPower.LastStatementReceived = foreignPower.StatementReceived;
                        foreignPower.StatementReceived = null;
                    }
                    #endregion Statements

                    #region Responses
                    // Response 
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
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyAccept, +100));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +100);
                            otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyAccept, +100));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, +100);
                        }

                        if (foreignPower.ResponseReceived.ResponseType == ResponseType.Reject)
                        {
                            foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyReject, -10));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -10);
                            otherForeignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.TreatyReject, -10));
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, -10);
                        }

                        if (foreignPower.ResponseReceived.ResponseType == ResponseType.Counter)
                        {
                            //foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +25));
                            //DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +50);
                            //otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +50));
                            //DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, +50);
                        }

                        if (foreignPower.ResponseReceived.ResponseType == ResponseType.NoResponse) // do we need this?
                        {
                            //foreignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.BorderIncursionPullout, +0));
                            //DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +0);
                        }

                        foreignPower.LastResponseReceived = foreignPower.ResponseReceived;
                        foreignPower.ResponseReceived = null;

                    }
                    #endregion Responses 

                    foreignPower.UpdateRegardAndTrustMeters();
                    foreignPower.UpdateStatus();
                }
            }

        }
        #region methods
        public static void TrustAndRegardByTraits( ForeignPower foreignP, int impact, int similarTraits)
        {
            if (similarTraits == 10)
            {
                foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                    75 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner,
                    55 + impact);
            }
            else if (similarTraits == 6)
            {
                foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                    55 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner,
                    40 + impact);
            }
            else if (similarTraits == 5)
            {
                foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                    30 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 20 + impact);
            }
            else if (similarTraits == 3)
            {
                foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                      10 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner,
                     10 + impact);
            }
            else if (similarTraits == 0)
            {
                foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                    -90 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner,
                    -85 + impact);
            }
        }
        public static void TrustAndRegardForATrait(ForeignPower foreignPow, int degree, string[] traits, string[] otherTraits)
        {
            if (traits.Contains("Warlike"))
            {
                if (otherTraits.Contains("Warlike"))
                    degree = 10;
                else if (otherTraits.Contains("Pleaceful"))
                    degree = -10;
                foreignPow.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, (-25 + degree)));
                DiplomacyHelper.ApplyTrustChange(foreignPow.Counterparty, foreignPow.Owner, (-25 + degree));
            }
            else if(traits.Contains("Peaceful"))
            {
                if (otherTraits.Contains("Peaceful"))
                    degree = -20;
                else if (otherTraits.Contains("Warlike"))
                    degree = 20;
                foreignPow.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, (-30 + degree)));
                DiplomacyHelper.ApplyTrustChange(foreignPow.Counterparty, foreignPow.Owner, (-30 + degree));
            }
        }
        #endregion   
    }
}