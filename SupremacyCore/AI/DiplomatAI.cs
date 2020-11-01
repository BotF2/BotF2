using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using Microsoft.Practices.ServiceLocation;
using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Diplomacy;
using Supremacy.Diplomacy.Visitors;
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
               // GameLog.Client.Diplomacy.DebugFormat("## similar traits ={0} counterparty ={1} traits ={2} owner ={3} traits ={4}",
                    //similarTraits, foreignPower.Counterparty.Key,foreignPower.Counterparty.Traits, foreignPower.Owner.Key, foreignPower.Owner.Traits );

                /*
                 * look for human to human proposals
                 */
                //if (aCiv.IsHuman && otherCiv.IsHuman)
                //{
                //    GameLog.Client.Diplomacy.DebugFormat("$$ HUMAN counterparty {0} to HUMAN owner {1}...",
                //        foreignPower.Counterparty.Key, foreignPower.Owner.Key);
                //    if (foreignPower.ProposalReceived != null)
                //    {
                //        if (aCiv == foreignPower.ProposalReceived.Recipient)
                //        {
                //            foreach (var clause in foreignPower.ProposalReceived.Clauses)
                //            {
                //                GameLog.Client.Diplomacy.DebugFormat("$$ Clause {0} duration {1}",
                //                        clause.ClauseType.ToString(), clause.Duration);                                      
                //            }
                //        }
                //    }
                //}

                if (true)//(!aCiv.IsHuman)
                {
                    //GameLog.Client.Diplomacy.DebugFormat("## Beging DiplomacyAI for aCiv AI .......................");
                    #region First Impression
                    /*
                     First impression delta trust and regard by traits
                    */
                    if (!foreignPower.DiplomacyData.FirstDiplomaticAction)
                    {
                        foreignPower.DiplomacyData.FirstDiplomaticAction = true;
                        int impact = 75;
                        var coutnerParty = foreignPower.Counterparty.CivID;
                        switch (coutnerParty)
                        {
                            case 0: //fed
                                {
                                    impact = 95;
                                    break;
                                }
                            case 1: // terran
                                {
                                    impact = 60;
                                    break;
                                }
                            case 4: // card
                                {
                                    impact = 65;
                                    break;
                                }
                            case 5: // dom
                                {
                                    impact = 60;
                                    break;
                                }
                            default:
                                break;
                        }
                 
                        GameLog.Client.Diplomacy.DebugFormat("## To = {0} regard ={2} trust ={3} Before First Impression fropm {1}",
                            foreignPower.Counterparty.Key,
                              foreignPower.Owner.Key,
                              foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue,
                              foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue);
                        TrustAndRegardByTraits(foreignPower, impact, similarTraits);

                        GameLog.Client.Diplomacy.DebugFormat("## To = {0} regard ={2} trust ={3} After First Impression from {1}",
                            foreignPower.Counterparty.Key,
                              foreignPower.Owner.Key,
                              foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue,
                              foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue);
                        //GameLog.Client.Diplomacy.DebugFormat("## foreignPower CounterParty ={0} regard ={1} trust ={2}", foreignPower.Counterparty.Key, foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue, foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue);
                        //GameLog.Client.Diplomacy.DebugFormat("## foreignPower .......Owner ={0} regard ={1} trust ={2}", foreignPower.Owner.Key, foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.DiplomacyData.Trust.CurrentValue);
                        //foreignPower.UpdateStatus();
                        //GameLog.Client.Diplomacy.DebugFormat("## current aCiv ={0} otherCiv ={1} foreighPower.Counterparty ={2} foreighPower.Owner ={3}",
                        //    aCiv.ShortName, otherCiv.ShortName, foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty Status {0} Owner Status {1}",
                        //    foreignPower.CounterpartyDiplomacyData.Status.ToString(),
                        //    foreignPower.DiplomacyData.Status.ToString());
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty Regard={0} Trust={1} Owner Regard={2} Trust={3}",
                        //    foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue,
                        //    foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue,
                        //    foreignPower.DiplomacyData.Regard.CurrentValue,
                        //    foreignPower.DiplomacyData.Trust.CurrentValue);
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty effective regard ={0} ", foreignPower.CounterpartyDiplomacyData.EffectiveRegard.ToString());
                    }
                    #endregion First Impressions

                    #region Ongoing Impressions

                    // if no other changes some variation over time
                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-3, 3));
                    DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-3, 3));                    

                    if ((5 - foreignPower.DiplomacyData.LastColdWarAttack) < 0 || 4 - foreignPower.DiplomacyData.LastIncursion < 0 || 6 - foreignPower.DiplomacyData.LastTotalWarAttack < 0)
                    {
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-4, 10));
                        DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-1, 7));
                    }
                    if (GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyNonAggression) != null)
                    {
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(1, 12));
                        DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(1, 7));
                    }
                    if (GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyOpenBorders) != null ||
                        GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyDefensiveAlliance) != null ||
                        GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyAffiliation) != null)
                    {
                        DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(3, 12));
                        DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(2, 10));
                    }
                    //foreignPower.UpdateRegardAndTrustMeters();
                    GameLog.Client.Diplomacy.DebugFormat("## To = {0} regard ={2} trust ={3} After Ongoing Impression from {1}",
                        foreignPower.Counterparty.Key,
                          foreignPower.Owner.Key,
                          foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue,
                          foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue);
                    // GameLog.Client.Diplomacy.DebugFormat("## foreignPower .......Owner ={0} regard ={1} trust ={2} After Ongoing Impression change", foreignPower.Owner.Key, foreignPower.DiplomacyData.Regard.CurrentValue, foreignPower.DiplomacyData.Trust.CurrentValue);
                    //foreignPower.UpdateStatus();
                    #endregion

                    #region Proposal Treaty to AI aCiv
                    /*
                      proposals TREATY
                    */
                    if (foreignPower.ProposalReceived != null)
                    {
                        if (aCiv == foreignPower.ProposalReceived.Recipient)
                        {// give credit regard and trust
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
                                    foreignPower.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent, value / 2 + greedy));
                                    DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, value / 3 + greedy);
                                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, value / 2 + greedy);
                                }
                                if (clause.ClauseType == ClauseType.RequestGiveCredits)
                                {
                                    DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-80, -100));
                                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-85, -120));
                                }
                                if (clause.ClauseType == ClauseType.TreatyCeaseFire)
                                {
                                    DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(180, 210));
                                    DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(170, 230));
                                }
                            }
                        }

                        //GameLog.Client.Diplomacy.DebugFormat(
                        //    "## foreignPower PendingAction ={0} Counterparty = {1} Onwer = {2}",
                        //    foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                        //    foreignPower.Owner.ShortName);

                        //if (foreignPower.DiplomacyData.Status == ForeignPowerStatus.Affiliated)
                        /*
                         AI evaluates accept reject
                         */
                        if (foreignPower.ProposalReceived != null && !aCiv.IsHuman) // aCiv is owner of the foreignpower looking for a ProposalRecieved
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
                            /*
                            /switch in GameEngine picks up PendingAction on next turn and calls AcceptProposalVisitor.Visit(ForeignPower.LastProposalReceived); and Reject...
                            */
                            if (accepted == true)
                            {
                                foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;

                                //GameLog.Client.Diplomacy.DebugFormat(
                                //    "## PendingAction: ACCEPT ={0} reset by clause - regard value, Counterparty = {1} Onwer = {2}",
                                //    foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                                //    foreignPower.Owner.ShortName);
                            }
                            else
                            {
                                foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;

                                //GameLog.Client.Diplomacy.DebugFormat(
                                //    "## PendingAction: REJECT ={0} reset by clause - regard value, Counterparty = {1} Onwer = {2}",
                                //    foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                                //    foreignPower.Owner.ShortName);
                            }
                        }
                       // foreignPower.UpdateRegardAndTrustMeters();

                        GameLog.Client.Diplomacy.DebugFormat("## To = {0} regard ={2} trust ={3} After Treaties from {1}",
                            foreignPower.Counterparty.Key,
                              foreignPower.Owner.Key,
                              foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue,
                              foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue);

                        #endregion Proposals
                    }
                   // foreignPower.UpdateRegardAndTrustMeters();
                   //foreignPower.UpdateStatus();// this is done in AcceptProposalVisitor.Visit
                }

                if (true) // for human and non human alike )
                {
                    //GameLog.Client.Diplomacy.DebugFormat("## Begin Statements, Human and AI civs .............................");
                    // did proposals received (incoming) now Statements outgoing

                    //GameLog.Client.Diplomacy.DebugFormat("## current ..................aCiv ={0} ...............otherCiv ={1}",
                    //        aCiv.ShortName, otherCiv.ShortName);
                    //GameLog.Client.Diplomacy.DebugFormat("## otherForeignPower.Counterparty ={0} otherForeignPower.Owner ={1}",
                    //    otherForeignPower.Counterparty.ShortName, otherForeignPower.Owner.ShortName);
                    //GameLog.Client.Diplomacy.DebugFormat("## .....foreignPower.Counterparty ={0} .....foreignPower.Owner ={1}", 
                    //    foreignPower.Counterparty.ShortName, foreignPower.Owner.ShortName);

                    #region Statments

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
                            DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, -1000); //foreignPower.Counterparty is civ that gets a degraded regard and foreignPower.Owner is civilization where degraded regard is owned (happens for)
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -1000);

                            List<Civilization> otherReactors = DiplomacyHelper.FindOtherContactedCivsForDeltaRegardTrust(foreignPower.Counterparty, foreignPower.Owner);
                            if (otherReactors != null)
                            {
                                foreach (Civilization anotherCiv in otherReactors)
                                {
                                    var counterparty = foreignPower.Counterparty;
                                    var owner = foreignPower.Owner;
                                    Statement denounceStatement = new Statement(anotherCiv, foreignPower.Counterparty, StatementType.DenounceWar, Tone.Enraged, GameContext.Current.TurnNumber);
                                    Statement commendStatement = new Statement(anotherCiv, foreignPower.Counterparty, StatementType.CommendWar, Tone.Enthusiastic, GameContext.Current.TurnNumber);
                                    var anotherDiplomat = Diplomat.Get(anotherCiv);
                                    var anotherForeignPower = anotherDiplomat.GetForeignPower(counterparty);

                                    if (DiplomacyHelper.IsAlliedWithWorstEnemy(counterparty, anotherCiv))
                                    {
                                        if (!anotherCiv.IsHuman)
                                        {
                                            anotherForeignPower.StatementSent = denounceStatement;
                                            anotherForeignPower.CounterpartyForeignPower.StatementReceived = denounceStatement;
                                            anotherForeignPower.DenounceWar(owner);
                                        }
                                        DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, -1000); //foreignPower.Counterparty is civ that gets a degraded regard and foreignPower.Owner is civilization where degraded regard is owned (happens for)
                                        DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, -1000);
                                        if (DiplomacyHelper.AreFriendly(owner, anotherCiv))
                                        {
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, +110);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, +90);
                                        }
                                    }
                                    if (DiplomacyHelper.AreNotFriendly(counterparty, anotherCiv))
                                    {
                                        if (DiplomacyHelper.AreFriendly(owner, anotherCiv))
                                        {
                                            if (!anotherCiv.IsHuman)
                                            {
                                                anotherForeignPower.StatementSent = commendStatement;
                                                anotherForeignPower.CounterpartyForeignPower.StatementReceived = commendStatement;
                                                anotherForeignPower.CommendWar(owner);
                                            }
                                            DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, -200);
                                            DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, -210);
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, +70);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, +50);
                                        }
                                        else if (DiplomacyHelper.AreNotFriendly(owner, anotherCiv))
                                        {
                                            DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, -100);
                                            DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, -110);
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, +50);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, +60);
                                        }
                                    }
                                    else if (DiplomacyHelper.AreFriendly(counterparty, anotherCiv))
                                    {
                                        if (DiplomacyHelper.AreNotFriendly(owner, anotherCiv))
                                        {
                                            if (!anotherCiv.IsHuman)
                                            {
                                                anotherForeignPower.StatementSent = denounceStatement;
                                                anotherForeignPower.CounterpartyForeignPower.StatementReceived = denounceStatement;
                                                anotherForeignPower.DenounceWar(foreignPower.Owner);
                                            }
                                            DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, +110);
                                            DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, +110);
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, -210);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, -170);
                                        }
                                    }
                                    else if (DiplomacyHelper.AreNeutral(counterparty, anotherCiv))
                                    {
                                        if (DiplomacyHelper.AreNotFriendly(owner, anotherCiv))
                                        {
                                            DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, +150);
                                            DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, +130);
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, -190);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, -170);
                                        }
                                        else if (DiplomacyHelper.AreFriendly(owner, anotherCiv))
                                        {
                                            DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, -100);
                                            DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, -150);
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, +50);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, +70);
                                        }
                                    }                                
                                }
                            }
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

                        //if (foreignPower.StatementReceived.StatementType == StatementType.DenounceWar)
                        //    //|| foreignPower.StatementReceived.StatementType == StatementType.DenounceSabotage
                        //    //|| foreignPower.StatementReceived.StatementType == StatementType.DenounceInvasion
                        //    //|| foreignPower.StatementReceived.StatementType == StatementType.DenounceAssault)
                        //{
                        //    //TrustAndRegardByTraits(similarTraits, foreignPower, impact);
                        //}

                        //if (foreignPower.StatementReceived.StatementType == StatementType.CommendWar)
                        //    //||foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                        //    //|| foreignPower.StatementReceived.StatementType == StatementType.CommendAssault
                        //    //|| foreignPower.StatementReceived.StatementType == StatementType.CommendRelationship
                        //    //|| foreignPower.StatementReceived.StatementType == StatementType.CommendSabotage)
                        //{
                        //    //TrustAndRegardByTraits(similarTraits, foreignPower, impact);
                        //}
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
                            DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, +90);
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +80);
                        }

                        if (foreignPower.ResponseReceived.ResponseType == ResponseType.Reject)
                        {
                            DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, -5);
                            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, -10);
                        }

                        //if (foreignPower.ResponseReceived.ResponseType == ResponseType.Counter)
                        //{
                        //    //foreignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +25));
                        //    //DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +50);
                        //    //otherForeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +50));
                        //    //DiplomacyHelper.ApplyTrustChange(foreignPower.Owner, foreignPower.Counterparty, +50);
                        //}

                        //if (foreignPower.ResponseReceived.ResponseType == ResponseType.NoResponse) // do we need this?
                        //{
                        //    //foreignPower.AddRegardEvent(new RegardEvent(1, RegardEventType.BorderIncursionPullout, +0));
                        //    //DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, +0);
                        //}

                        foreignPower.LastResponseReceived = foreignPower.ResponseReceived;
                        foreignPower.ResponseReceived = null;

                    }
                    #endregion Responses 

                    //foreignPower.UpdateRegardAndTrustMeters();
                    //foreignPower.UpdateStatus();
                }
            }

        }
        #region methods

        public static void TrustAndRegardByTraits( ForeignPower foreignP, int impact, int similarTraits)
        {
            if (similarTraits == 10)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //    75 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 55 + impact);
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, 40 + impact);
            }
            else if (similarTraits == 6)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //    55 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 40 + impact);
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, 30 + impact);
            }
            else if (similarTraits == 5)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //    30 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 20 + impact);
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, 10 + impact);
            }
            else if (similarTraits == 3)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //      10 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, -15 + impact);
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, -20 + impact);
            }
            else if (similarTraits == 0)
            {
            //    foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
            //        -90 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, -95 + impact);
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, -90 + impact);
            }
        }
        public static void TrustAndRegardForATrait(ForeignPower foreignPow, int degree, string[] traits, string[] otherTraits)
        {
            if (traits.Contains("Warlike"))
            {
                if (otherTraits.Contains("Warlike"))
                    degree = 20;
                else if (otherTraits.Contains("Pleaceful"))
                    degree = -25;
                DiplomacyHelper.ApplyRegardChange(foreignPow.Counterparty, foreignPow.Owner, degree);
                DiplomacyHelper.ApplyTrustChange(foreignPow.Counterparty, foreignPow.Owner, degree);
            }
            else if(traits.Contains("Peaceful"))
            {
                if (otherTraits.Contains("Peaceful"))
                    degree = -25;
                else if (otherTraits.Contains("Warlike"))
                    degree = 20;
                DiplomacyHelper.ApplyRegardChange(foreignPow.Counterparty, foreignPow.Owner, degree);
                DiplomacyHelper.ApplyTrustChange(foreignPow.Counterparty, foreignPow.Owner, degree);
            }
        }
        private static readonly Random getrandom = new Random();

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) // synchronize
            {
                return getrandom.Next(min, max);
            }
        }

        #endregion   
    }
}