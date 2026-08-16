// File:DiplomatAI.cs
using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Supremacy.AI
{

    public static class DiplomatAI
    {
        //#pragma warning disable IDE0044 // Modifizierer "readonly" hinzufügen
        private static List<ForeignPower> AlreadyMinorMember = new List<ForeignPower>();
        //#pragma warning restore IDE0044 // Modifizierer "readonly" hinzufügen
        public static void DoTurn([NotNull] ICivIdentity civ) // pass in all civs to process Diplomacy + Sabotage
        {
            string _diploSummary = "";
            bool _bool_DoSabotage = false;
            string _text = "";
            var _sb = new StringBuilder();
            //string _newline = Environment.NewLine;

            if (civ == null)
            {
                throw new ArgumentNullException("civ");
            }

            Civilization _civ1 = (Civilization)civ;
            Diplomat _diplomat_1 = Diplomat.Get(civ);
            bool _bool_isHuman_player = _civ1.IsHuman;

            /*
             * Process messages which have already been delivered
             */
            foreach (Civilization _civ2 in GameContext.Current.Civilizations)
            // we can control _regard and _trust for both human otherCivs and AI otherCivs
            {
                if (_civ2.CivID == _civ1.CivID)
                {
                    continue;
                }

                if (!DiplomacyHelper.IsContactMade(_civ1.CivID, _civ2.CivID))
                {
                    continue;
                }

                //if (!_civ2.IsEmpire && !_civ1.IsEmpire)
                //{
                //    continue; // is Minor
                //}

                ForeignPower _foreign_power_1 = _diplomat_1.GetForeignPower(_civ2);
                Diplomat _diplomat_2 = Diplomat.Get(_civ2);
                ForeignPower _foreign_power_2 = _diplomat_2.GetForeignPower(_civ1);
                string _civ2Name = GameEngine.Do_x_String(15, _civ2.Key);

                _text = "Step_5405:"
                    + "; _regard= " + GameEngine.Do_x_Digit_String(4, _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue.ToString())
                    + "; _trust= " + GameEngine.Do_x_Digit_String(4, _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue.ToString())
                    + "; Status= * " + GameEngine.Do_x_String(26, _foreign_power_1.DiplomacyData.Status.ToString())
                    + _civ1.Key
                    + " vs " + _civ2Name
                    //+ " (DiplomatAI.cs)" 
                    + " * > Traits= (not listed)"// + _civ1.Traits
                    //+ " - vs - " + _civ2.Traits
                    ;
                Console.WriteLine(_text);
                _diploSummary += Environment.NewLine + _text;

                if (_foreign_power_1.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember
                    || _foreign_power_2.DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
                {
                    continue;  // is Member
                }

                //CheckTraits(_civ2, _civ1);

                string traitsOfForeignCiv = _civ2.Traits;
                string[] foreignTraits = traitsOfForeignCiv.Split(',');


                string traitsOfCiv = _civ1.Traits;
                string[] theCivTraits = traitsOfCiv.Split(',');


                // traits in common relative to the number of triats a civilization has
                IEnumerable<string> commonTraitItems = foreignTraits.Intersect(theCivTraits);

                int countCommon = 0;
                foreach (string aString in commonTraitItems)
                {
                    countCommon++;
                }

                int[] countArray = new int[] { foreignTraits.Length, theCivTraits.Length };
                int fewestTotalTraits = countArray.Min();

                int similarTraits = countCommon * 10 / fewestTotalTraits; // (a double from 1 to 0) * 10 
                                                                          // GameLog.Client.Diplomacy.DebugFormat("## similar traits ={0} counterparty ={1} traits ={2} owner ={3} traits ={4}",
                                                                          //similarTraits, _foreign_power_1.Counterparty.Key,_foreign_power_1.Counterparty.Traits, _foreign_power_1.Owner.Key, _foreign_power_1.Owner.Traits );

                /*
                 * look for human to human proposals
                 */
                if (_civ1.IsHuman && _civ2.IsHuman)
                {
                    _text = "Step_2563:; Human to Human ( "
                        + _civ1.Key
                        + " vs " + _civ2.Key
                        ;
                    Console.WriteLine(_text);
                    //GameLog.Client.Diplomacy.DebugFormat(_text);

                    if (_foreign_power_1.ProposalReceived != null)
                    {
                        if (_civ1 == _foreign_power_1.ProposalReceived.Recipient)
                        {
                            foreach (var clause in _foreign_power_1.ProposalReceived.Clauses)
                            {
                                string _how_long = "";

                                _text = "Step_2564:; > " + clause.ClauseType.ToString();
                                if (clause.Duration > 0)
                                {
                                    _text += " for " + clause.Duration + " turns ";
                                }
                                Console.WriteLine(_text);
                                //GameLog.Client.Diplomacy.DebugFormat(_text);
                            }
                        }
                    }
                }

                if (_bool_isHuman_player)
                {
                    //Debugger.Break();
                }

                if (!_civ1.IsHuman)  // it's not human controlled !
                {
                    //_text = "## Beging DiplomacyAI ......................." + _foreign_power_1.Owner + " vs " + _foreign_power_1.Counterparty;
                    //Console.WriteLine(_text);
                    //GameLog.Client.Diplomacy.DebugFormat("## Beging DiplomacyAI for _civ1 AI .......................");
                    //#region First Impression
                    /*
                     First impression delta _trust and _regard by traits
                    */
                    if (!_foreign_power_1.DiplomacyData.FirstDiplomaticAction)
                    {
                        _foreign_power_1.DiplomacyData.FirstDiplomaticAction = true;
                        int impact = 75;
                        int coutnerParty = _foreign_power_1.Counterparty.CivID;
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

                        //GameLog.Client.DiplomacyDetails.DebugFormat("## To = {0} _regard ={2} _trust ={3} Before First Impression from {1}",
                        _text = "Step_1171:; Turn: " + GameContext.Current.TurnNumber
                            + "; _regard= " + _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue
                            + "; _trust= " + _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue
                            + " for " + _foreign_power_1.Counterparty.Key
                            + " BEFORE First Impression from " + _foreign_power_1.Owner.Key
                            ;
                        Console.WriteLine(_text);
                        _diploSummary += Environment.NewLine + _text;
                        //GameLog.Client.DiplomacyDetails.DebugFormat(_text);


                        TrustAndRegardByTraits(_foreign_power_1, impact, similarTraits);


                        _text = "Step_1173:; Turn: " + GameContext.Current.TurnNumber
                            + ": _regard= " + _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue
                            + ", _trust= " + _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue
                            + " for " + _foreign_power_1.Counterparty.Key
                            + " AFTER First Impression from " + _foreign_power_1.Owner.Key
                            ;
                        Console.WriteLine(_text);
                        _diploSummary += Environment.NewLine + _text;
                        //GameLog.Client.DiplomacyDetails.DebugFormat(_text);


                        //GameLog.Client.Diplomacy.DebugFormat("## _foreign_power_1 CounterParty ={0} _regard ={1} _trust ={2}", _foreign_power_1.Counterparty.Key, _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue, _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue);
                        //GameLog.Client.Diplomacy.DebugFormat("## _foreign_power_1 .......Owner ={0} _regard ={1} _trust ={2}", _foreign_power_1.Owner.Key, _foreign_power_1.DiplomacyData.Regard.CurrentValue, _foreign_power_1.DiplomacyData.Trust.CurrentValue);
                        //_foreign_power_1.UpdateStatus();
                        //GameLog.Client.Diplomacy.DebugFormat("## current _civ1 ={0} _civ2 ={1} foreighPower.Counterparty ={2} foreighPower.Owner ={3}",
                        //    _civ1.ShortName, _civ2.ShortName, _foreign_power_1.Counterparty.ShortName, _foreign_power_1.Owner.ShortName);
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty Status {0} Owner Status {1}",
                        //    _foreign_power_1.CounterpartyDiplomacyData.Status.ToString(),
                        //    _foreign_power_1.DiplomacyData.Status.ToString());
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty Regard={0} Trust={1} Owner Regard={2} Trust={3}",
                        //    _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue,
                        //    _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue,
                        //    _foreign_power_1.DiplomacyData.Regard.CurrentValue,
                        //    _foreign_power_1.DiplomacyData.Trust.CurrentValue);
                        //GameLog.Client.Diplomacy.DebugFormat("## Counterparty effective _regard ={0} ", _foreign_power_1.CounterpartyDiplomacyData.EffectiveRegard.ToString());
                    }
                    //#endregion First Impressions

                    #region Ongoing Impressions

                    //Debugger.Break();

                    Do_Ongoing_Regard_Trust(_foreign_power_1, _civ2);

                    //// if no other changes some variation over time
                    //DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(-3, 3));
                    //DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(-3, 3));

                    //if ((5 - _foreign_power_1.DiplomacyData.LastColdWarAttack) < 0 || 4 - _foreign_power_1.DiplomacyData.LastIncursion < 0 || 6 - _foreign_power_1.DiplomacyData.LastTotalWarAttack < 0)
                    //{
                    //    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(-4, 10));
                    //    DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(-1, 7));
                    //}
                    //// TreatyNonAggression
                    //if (GameContext.Current.AgreementMatrix.FindAgreement(_civ2, _foreign_power_1, ClauseType.TreatyNonAggression) != null)
                    //{
                    //    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(1, 12));
                    //    DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(1, 7));
                    //}
                    //// OpenBorders or TreatyDefensiveAlliance or TreatyAffiliation
                    //if (GameContext.Current.AgreementMatrix.FindAgreement(_civ2, _foreign_power_1, ClauseType.TreatyOpenBorders) != null ||
                    //    GameContext.Current.AgreementMatrix.FindAgreement(_civ2, _foreign_power_1, ClauseType.TreatyDefensiveAlliance) != null ||
                    //    GameContext.Current.AgreementMatrix.FindAgreement(_civ2, _foreign_power_1, ClauseType.TreatyAffiliation) != null)
                    //{
                    //    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(3, 12));
                    //    DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(2, 10));
                    //}
                    //_foreign_power_1.UpdateRegardAndTrustMeters();


                    //_text = "Step_xxxx:; Turn " + GameContext.Current.TurnNumber
                    //    + ": _regard= " + _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue
                    //    + ", _trust= " + _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue
                    //    + " for " + _foreign_power_1.Counterparty.Key
                    //    + " AFTER Ongoing Impression " + _foreign_power_1.Owner.Key
                    //    ;
                    //Console.WriteLine(_text);
                    //_diploSummary = _newline + _text;
                    //GameLog.Client.DiplomacyDetails.DebugFormat(_text);

                    // GameLog.Client.Diplomacy.DebugFormat("## _foreign_power_1 .......Owner ={0} _regard ={1} _trust ={2} After Ongoing Impression change", _foreign_power_1.Owner.Key, _foreign_power_1.DiplomacyData.Regard.CurrentValue, _foreign_power_1.DiplomacyData.Trust.CurrentValue);
                    _foreign_power_1.UpdateStatus();
                    #endregion

                    if (_foreign_power_1.DiplomacyData.Status < ForeignPowerStatus.Peace)
                    {
                        if (_civ1.SpiedCivList != null && _civ1.SpiedCivList.Contains(_civ2))
                        {
                            _bool_DoSabotage = true;
                        }
                    }


                    //if (_bool_DoSabotage)
                    //{
                    //DoSabotage(_foreign_power_1, _civ2);
                    //}



                    #region War is possible from hostility

                    // old: Hostile AND ShouldTheyGoToWar
                    //if (_foreign_power_1.DiplomacyData.Status == ForeignPowerStatus.Hostile
                    if (_foreign_power_1.DiplomacyData.Regard.CurrentValue < 430
                        && _foreign_power_1.DiplomacyData.Status != ForeignPowerStatus.AtWar
                        && DiplomacyHelper.ShouldTheyGoToWar(_foreign_power_1.Owner, _foreign_power_1.Counterparty)) //_foreign_power_1.DiplomacyData.Status == ForeignPowerStatus.Hostile &&
                    {
                        Civilization _civ_1 = _foreign_power_1.Owner;
                        Civilization _civ_2 = _foreign_power_1.Counterparty;
                        CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ_1];
                        CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ_2];
                        _foreign_power_1.DeclareWar();
                        _bool_DoSabotage = true;
                        _civM_1.SitRepEntries.Add(new WarDeclaredSitRepEntry(_civ_1, _civ_2));
                        _civM_2.SitRepEntries.Add(new WarDeclaredSitRepEntry(_civ_1, _civ_2));
                        DiplomacyHelper.ApplyTrustChange(_civ_1, _civ_2, _foreign_power_1.DiplomacyData.Trust.CurrentValue * -1);
                        DiplomacyHelper.ApplyRegardChange(_civ_2, _civ_1, _foreign_power_1.CounterpartyForeignPower.DiplomacyData.Regard.CurrentValue * -1);
                    }
                    #endregion

                    _text = "#region Proposal Treaty to AI aCiv";
                    /*
                      proposals TREATY
                    */
                    if (_foreign_power_1.ProposalReceived != null)
                    {
                        if (_civ1 == _foreign_power_1.ProposalReceived.Recipient)
                        {
                            // give credit _regard and _trust
                            foreach (IClause clause in _foreign_power_1.ProposalReceived.Clauses)
                            {
                                if (clause.ClauseType == ClauseType.OfferCredits)
                                {
                                    int value = (((CreditsClauseData)clause.Data).ImmediateAmount +
                                                 ((CreditsClauseData)clause.Data).RecurringAmount) / 25;
                                    int greedy = 0;
                                    if (_foreign_power_1.ProposalReceived.Recipient.Traits.Contains("Materialistic"))
                                    {
                                        greedy = 50;
                                    }
                                    _foreign_power_1.AddRegardEvent(new RegardEvent(5, RegardEventType.NoRegardEvent, value / 2 + greedy));
                                    DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, value / 3 + greedy);
                                    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, value / 2 + greedy);
                                }
                                if (clause.ClauseType == ClauseType.RequestCredits)
                                {
                                    DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(-100, -80)); // lower before !
                                    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(-120, -85));
                                }
                                if (clause.ClauseType == ClauseType.TreatyCeaseFire)
                                {
                                    DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(180, 210));
                                    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, GetRandomNumber(170, 230));
                                }
                            }
                        }

                        _text = "Step_2653:; "
                            + _foreign_power_1.Owner.ShortName
                            + " has PendingAction= " + _foreign_power_1.PendingAction.ToString()
                            + " from Counterparty= " + _foreign_power_1.Counterparty.ShortName


                            ;
                        Console.WriteLine(_text);
                        //GameLog.Client.Diplomacy.DebugFormat(_text);
                        //GameLog.Client.Diplomacy.DebugFormat(
                        //    "## _foreign_power_1 PendingAction ={0} Counterparty = {1} Onwer = {2}",
                        //    _foreign_power_1.PendingAction.ToString(), _foreign_power_1.Counterparty.ShortName,
                        //    _foreign_power_1.Owner.ShortName);

                        //if (_foreign_power_1.DiplomacyData.Status == ForeignPowerStatus.Affiliated)


                        /*
                         AI evaluates accept reject
                         */
                        if (_foreign_power_1.ProposalReceived != null
                            && !_civ1.IsHuman) // _civ1 is owner of the foreignpower looking for a ProposalRecieved
                        {
                            bool _accepted = false;
                            int _regard = _foreign_power_1.DiplomacyData.Regard.CurrentValue;
                            int _trust = _foreign_power_1.DiplomacyData.Regard.CurrentValue;
                            //bool traits = RandomHelper.Chance(similarTraits);

                            // first check for credits (gift or demand)
                            foreach (IClause clause in _foreign_power_1.ProposalReceived.Clauses)
                            {
                                switch (clause.ClauseType)
                                {
                                    case ClauseType.OfferCredits:
                                        //int credits = clause.Data;
                                        break;
                                    //case ClauseType.TreatyWarPact
                                    case ClauseType.RequestCredits:
                                        break;
                                }
                            }

                            foreach (IClause clause in _foreign_power_1.ProposalReceived.Clauses)
                            {

                                switch (clause.ClauseType)
                                {
                                    case ClauseType.TreatyMembership:

                                        if (_regard > 899 && _trust > 899 && !AlreadyMinorMember.Contains(_foreign_power_1))
                                        {
                                            _accepted = true;
                                            AlreadyMinorMember.Add(_foreign_power_1);
                                        }
                                        _text = "Step_2666:; "
                                            + " >>> Membership _accepted= " + _accepted
                                            + ", Sender was= " + _foreign_power_1.ProposalReceived.Sender
                                            + ", Recipient= " + _foreign_power_1.ProposalReceived.Recipient

                                            ;
                                        Console.WriteLine(_text);

                                        break;

                                    case ClauseType.TreatyFullAlliance:
                                        if (_regard > 899 && _trust > 899)
                                        {
                                            _accepted = true;
                                        }

                                        break;

                                    case ClauseType.TreatyDefensiveAlliance:
                                        if (_regard > 799 && _trust > 799)
                                        {
                                            _accepted = true;
                                        }

                                        break;

                                    case ClauseType.TreatyWarPact:
                                        if (_regard > 799 && _trust > 799)
                                        {
                                            _accepted = true;
                                        }

                                        break;

                                    case ClauseType.TreatyAffiliation:
                                        if (_regard > 699 && _trust > 699)
                                        {
                                            _accepted = true;
                                        }

                                        break;

                                    case ClauseType.TreatyNonAggression:
                                        if (_regard > 499 && _trust > 499)
                                        {
                                            _accepted = true;
                                        }

                                        break;

                                    case ClauseType.TreatyOpenBorders:
                                        if (_regard > 399 && _trust > 399)
                                        {
                                            _accepted = true;

                                            _text = "Step_2666:; "
                                                    + " >>> OpenBorders _accepted= " + _accepted
                                                    + ", Sender was= " + _foreign_power_1.ProposalReceived.Sender
                                                    + ", Recipient= " + _foreign_power_1.ProposalReceived.Recipient

                                                    ;
                                            Console.WriteLine(_text);
                                        }
                                        break;

                                    case ClauseType.TreatyCeaseFire:
                                        {
                                            Random num = new Random();
                                            int chance = num.Next(1, similarTraits + 2);
                                            if (chance != 1)
                                            {
                                                _accepted = true;
                                            }

                                            break;
                                        }

                                    // case for Credits are done before...

                                    //case ClauseType.OfferCredits:
                                    //    _accepted = true;
                                    //    break;
                                    ////case ClauseType.TreatyWarPact
                                    //case ClauseType.RequestCredits:
                                    //    break;

                                    default:
                                        break;
                                }
                            }


                            /*
                            /switch in GameEngine picks up PendingAction on next turn and calls AcceptProposalVisitor.Visit(ForeignPower.LastProposalReceived); and Reject...
                            */
                            if (_accepted == true)
                            {
                                _foreign_power_1.PendingAction = PendingDiplomacyAction.AcceptProposal;
                            }
                            else
                            {
                                _foreign_power_1.PendingAction = PendingDiplomacyAction.RejectProposal;
                            }
                        }
                        _foreign_power_1.UpdateRegardAndTrustMeters();

                        _text = "Step_3773:; Turn " + GameContext.Current.TurnNumber
                            + "; _regard =" + _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue
                            + "; _trust =" + _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue
                            + " AFTER Treaties from " + _foreign_power_1.Owner.Key
                            + " to " + _foreign_power_1.Counterparty.Key
                            ;
                        Console.WriteLine(_text);
                        _diploSummary += Environment.NewLine + _text;
                        //GameLog.Client.DiplomacyDetails.DebugFormat(_text);

                        _text = "#endregion Proposals";
                    }


                }

                if (true) // for human and non human alike )
                {
                    //GameLog.Client.Diplomacy.DebugFormat("## Begin Statements, Human and AI civs .............................");
                    // did proposals received (incoming) now Statements outgoing

                    //GameLog.Client.Diplomacy.DebugFormat("## current .................._civ1 ={0} ..............._civ2 ={1}",
                    //        _civ1.ShortName, _civ2.ShortName);
                    //GameLog.Client.Diplomacy.DebugFormat("## _foreign_power_2.Counterparty ={0} _foreign_power_2.Owner ={1}",
                    //    _foreign_power_2.Counterparty.ShortName, _foreign_power_2.Owner.ShortName);
                    //GameLog.Client.Diplomacy.DebugFormat("## ....._foreign_power_1.Counterparty ={0} ....._foreign_power_1.Owner ={1}", 
                    //    _foreign_power_1.Counterparty.ShortName, _foreign_power_1.Owner.ShortName);


                    #region Statements

                    if (_foreign_power_1.StatementReceived != null)
                    {
                        _text = "Step_3776:; Turn " + GameContext.Current.TurnNumber
                            + "; otherforeignPower.Statement (RECEIVED)= " + _foreign_power_1.StatementReceived.StatementType.ToString()
                            + "; Counterparty=" + _foreign_power_1.Counterparty.ShortName
                            + ";to; " + _foreign_power_1.Owner.ShortName
                            + "; Regard= " + _foreign_power_1.DiplomacyData.Regard.CurrentValue
                            + "; Trust= " + _foreign_power_1.DiplomacyData.Trust.CurrentValue
                            ;
                        Console.WriteLine(_text);
                        _diploSummary += Environment.NewLine + _text;
                        //GameLog.Client.Diplomacy.DebugFormat(_text);

                        // DOING: Process statements (apply _regard/_trust changes, etc.)
                        if (_foreign_power_1.StatementReceived.StatementType == StatementType.WarDeclaration)
                        {
                            DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, -1000); //_foreign_power_1.Counterparty is civ that gets a degraded _regard and _foreign_power_1.Owner is civilization where degraded _regard is owned (happens for)
                            DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, -1000);
                            List<Civilization> otherReactors = DiplomacyHelper.FindOtherContactedCivsForDeltaRegardTrust(_foreign_power_1.Counterparty, _foreign_power_1.Owner);
                            if (otherReactors != null)
                            {
                                foreach (Civilization anotherCiv in otherReactors)
                                {
                                    Civilization counterparty = _foreign_power_1.Counterparty;
                                    Civilization owner = _foreign_power_1.Owner;
                                    Statement denounceStatement = new Statement(anotherCiv, _foreign_power_1.Counterparty, StatementType.DenounceWar, Tone.Enraged, GameContext.Current.TurnNumber);
                                    Statement commendStatement = new Statement(anotherCiv, _foreign_power_1.Counterparty, StatementType.CommendWar, Tone.Enthusiastic, GameContext.Current.TurnNumber);
                                    Diplomat anotherDiplomat = Diplomat.Get(anotherCiv);
                                    ForeignPower anotherForeignPower = anotherDiplomat.GetForeignPower(counterparty);
                                    if (DiplomacyHelper.IsAlliedWithWorstEnemy(counterparty, anotherCiv))
                                    {
                                        if (!anotherCiv.IsHuman)
                                        {
                                            anotherForeignPower.StatementSent = denounceStatement;
                                            anotherForeignPower.CounterpartyForeignPower.StatementReceived = denounceStatement;
                                            anotherForeignPower.DenounceWar(owner);
                                        }
                                        DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, -1000); //_foreign_power_1.Counterparty is civ that gets a degraded _regard and _foreign_power_1.Owner is civilization where degraded _regard is owned (happens for)
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
                                                anotherForeignPower.DenounceWar(_foreign_power_1.Owner);
                                            }
                                            DiplomacyHelper.ApplyRegardChange(counterparty, anotherCiv, +110);
                                            DiplomacyHelper.ApplyTrustChange(counterparty, anotherCiv, +110);
                                            DiplomacyHelper.ApplyRegardChange(owner, anotherCiv, -210);
                                            DiplomacyHelper.ApplyTrustChange(owner, anotherCiv, -170);
                                        }
                                    }
                                    else if (DiplomacyHelper.Status_Neutral(counterparty, anotherCiv))
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
                            TrustAndRegardByTraits(_foreign_power_1, impact, similarTraits);
                            int degree = 0;
                            TrustAndRegardForATrait(_foreign_power_1, degree, foreignTraits, theCivTraits);




                            _text = "Step_3782:; Turn " + GameContext.Current.TurnNumber
                                    + "; WarDeclaration by counterparty= " + _foreign_power_1.Counterparty.ShortName
                                    + ";to; " + _foreign_power_1.Owner.ShortName
                                    + "; Regard= " + _foreign_power_1.DiplomacyData.Regard.CurrentValue
                                    + "; Trust= " + _foreign_power_1.DiplomacyData.Trust.CurrentValue
                                    ;
                            Console.WriteLine(_text);
                            _diploSummary += Environment.NewLine + _text;
                            //GameLog.Client.Diplomacy.DebugFormat(_text);

                            //GameLog.Client.Diplomacy.DebugFormat(
                            //    "$$$ After WarDeclaration by counterparty = {0} to {1} Regard = {2} Trust = {3}",
                            //    _foreign_power_1.Counterparty.ShortName, _foreign_power_1.Owner.ShortName,
                            //    _foreign_power_1.DiplomacyData.Regard.CurrentValue,
                            //    _foreign_power_1.DiplomacyData.Trust.CurrentValue);
                            //GameLog.Client.Diplomacy.DebugFormat(
                            //    "$$$ After WarDeclaration by counterparty their {0} Regard = {1} Trust = {2}",
                            //    _foreign_power_1.Counterparty.ShortName,
                            //    _foreign_power_2.DiplomacyData.Regard.CurrentValue,
                            //    _foreign_power_2.DiplomacyData.Trust.CurrentValue);
                        }

                        //if (_foreign_power_1.StatementReceived.StatementType == StatementType.ThreatenTradeEmbargo
                        //    || _foreign_power_1.StatementReceived.StatementType == StatementType.ThreatenDestroyColony
                        //    || _foreign_power_1.StatementReceived.StatementType == StatementType.ThreatenDeclareWar)
                        //{
                        //    _foreign_power_1.AddRegardEvent(new RegardEvent(20, RegardEventType.PeacetimeBorderIncursion, -500));
                        //    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, -300);
                        //    CounterpartyforeignPower.AddRegardEvent(new RegardEvent(10, RegardEventType.DeclaredWar, -200));
                        //    DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Owner, _foreign_power_1.Counterparty, -100);
                        //}

                        //if (_foreign_power_1.StatementReceived.StatementType == StatementType.DenounceWar)
                        //    //|| _foreign_power_1.StatementReceived.StatementType == StatementType.DenounceSabotage
                        //    //|| _foreign_power_1.StatementReceived.StatementType == StatementType.DenounceInvasion
                        //    //|| _foreign_power_1.StatementReceived.StatementType == StatementType.DenounceAssault)
                        //{
                        //    //TrustAndRegardByTraits(similarTraits, _foreign_power_1, impact);
                        //}

                        //if (_foreign_power_1.StatementReceived.StatementType == StatementType.CommendWar)
                        //    //||_foreign_power_1.StatementReceived.StatementType == StatementType.CommendAssault
                        //    //|| _foreign_power_1.StatementReceived.StatementType == StatementType.CommendAssault
                        //    //|| _foreign_power_1.StatementReceived.StatementType == StatementType.CommendRelationship
                        //    //|| _foreign_power_1.StatementReceived.StatementType == StatementType.CommendSabotage)
                        //{
                        //    //TrustAndRegardByTraits(similarTraits, _foreign_power_1, impact);
                        //}
                        // ToDo: look at AI civ reacting to blame with the StatementType.Sabotage... and Steal... 
                        //if (_foreign_power_1.StatementReceived.StatementType == StatementType.SabotageOrder) // only the borg now?
                        //    _foreign_power_1.AddRegardEvent(new RegardEvent(1, RegardEventType.NoRegardEvent, 0));

                        // if (_foreign_power_1.StatementReceived.StatementType == StatementType.NoStatement) // do we need something for this?

                        _foreign_power_1.LastStatementReceived = _foreign_power_1.StatementReceived;
                        _foreign_power_1.StatementReceived = null;
                    }
                    #endregion Statements

                    #region Responses
                    // Response 
                    if (_foreign_power_1.ResponseReceived != null)
                    {
                        // TODO: Process responses (apply _regard/_trust changes, etc.)

                        if (_foreign_power_1.ResponseReceived.ResponseType == ResponseType.Accept
                        )
                        {
                            GameLog.Client.Diplomacy.DebugFormat(
                                "## Responce type ={0} ResponseReceived by ?counterparty = {1} to {2} Regard = {3} Trust = {4}",
                                _foreign_power_1.ResponseReceived.ResponseType.ToString(),
                                _foreign_power_1.Counterparty.ShortName, _foreign_power_1.Owner.ShortName,
                                _foreign_power_1.DiplomacyData.Regard.CurrentValue,
                                _foreign_power_1.DiplomacyData.Trust.CurrentValue);
                        }
                        // Added some positive RegardEventTypes.
                        {
                            DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, +90);
                            DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, +80);
                        }

                        if (_foreign_power_1.ResponseReceived.ResponseType == ResponseType.Reject)
                        {
                            DiplomacyHelper.ApplyRegardChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, -5);
                            DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, -10);
                        }

                        //if (_foreign_power_1.ResponseReceived.ResponseType == ResponseType.Counter)
                        //{
                        //    //_foreign_power_1.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +25));
                        //    //DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, +50);
                        //    //_foreign_power_2.AddRegardEvent(new RegardEvent(10, RegardEventType.TreatyCounter, +50));
                        //    //DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Owner, _foreign_power_1.Counterparty, +50);
                        //}

                        //if (_foreign_power_1.ResponseReceived.ResponseType == ResponseType.NoResponse) // do we need this?
                        //{
                        //    //_foreign_power_1.AddRegardEvent(new RegardEvent(1, RegardEventType.BorderIncursionPullout, +0));
                        //    //DiplomacyHelper.ApplyTrustChange(_foreign_power_1.Counterparty, _foreign_power_1.Owner, +0);
                        //}

                        _foreign_power_1.LastResponseReceived = _foreign_power_1.ResponseReceived;
                        _foreign_power_1.ResponseReceived = null;

                    }
                    #endregion Responses 

                    _foreign_power_1.UpdateRegardAndTrustMeters();
                    _foreign_power_1.UpdateStatus();

                    //var
                    _sb = new StringBuilder();
                    _sb.Append("Step_5406:");
                    _sb.Append("; _regard= ");
                    _sb.Append(GameEngine.Do_x_Digit_String(4, _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue.ToString()));
                    _sb.Append("; _trust= ");
                    _sb.Append(GameEngine.Do_x_Digit_String(4, _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue.ToString()));
                    _sb.Append("; Status= * ");
                    _sb.Append(GameEngine.Do_x_String(26, _foreign_power_1.DiplomacyData.Status.ToString()));
                    _sb.Append(_civ1.Key);
                    _sb.Append(" vs ");
                    _sb.Append(_civ2Name);
                    _sb.Append(" * > Traits= (not listed)");
                    //_sb.Append(_civ1.Traits);
                    //_sb.Append(" - vs - ");
                    //_sb.Append(_civ2.Traits);

                    //_text = "Step_5406:"
                    //        + "; _regard= " + GameEngine.Do_x_Digit_String(4, _foreign_power_1.CounterpartyDiplomacyData.Regard.CurrentValue.ToString())
                    //        + "; _trust= " + GameEngine.Do_x_Digit_String(4, _foreign_power_1.CounterpartyDiplomacyData.Trust.CurrentValue.ToString())
                    //        + "; Status= * " + GameEngine.Do_x_String(26, _foreign_power_1.DiplomacyData.Status.ToString())
                    //        + _civ1.Key

                    //        + " vs " + _civ2Name
                    //        //+ " (DiplomatAI.cs)"

                    //        + " * > Traits= " + _civ1.Traits
                    //        + " - vs - " + _civ2.Traits
                    //        ;
                    Console.WriteLine(_sb.ToString());
                    _diploSummary += Environment.NewLine + _sb.ToString();
                }
            }
            Console.WriteLine(string.Concat("\r\nStep_7733:; Begin of _diploSummary", _diploSummary, "\r\nEnd of _diploSummary from Step_7733\r\n"));
        }

        //private static void DoSabotage(ForeignPower foreignPower, Civilization otherCiv)
        //{
        //    Debugger.Break();
        //    Debugger.Launch();
        //}

        private static void Do_Ongoing_Regard_Trust(ForeignPower foreignPower, Civilization otherCiv)
        {

            string _DoOngoingRegardTrust = "";
            string _text = "";
            //string _newline = Environment.NewLine;

            int _random_change = GetRandomNumber(-3, 3);

            _text = "Step_1174:;"
                    + " _regard= " + GameEngine.Do_x_Digit_String(4, foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue.ToString())
                    + ", _trust= " + GameEngine.Do_x_Digit_String(4, foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue.ToString())

                    + " > BEFORE Ongoing Impression: "

                    + "    for " + foreignPower.Owner.Key
                    + " vs " + foreignPower.Counterparty.Key
                    + " > Turn " + GameContext.Current.TurnNumber
                    + " > _random_change= " + _random_change
                    ;
            Console.WriteLine(_text); // BEFORE + AFTER seems to not being working



            // if no other changes some variation over time
            DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, _random_change);
            DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, _random_change);

            if ((5 - foreignPower.DiplomacyData.LastColdWarAttack) < 0
                || 4 - foreignPower.DiplomacyData.LastIncursion < 0
                || 6 - foreignPower.DiplomacyData.LastTotalWarAttack < 0)
            {
                DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-4, 10));
                DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(-1, 7));
            }

            // TreatyNonAggression
            if (GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyNonAggression) != null)
            {
                DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(1, 12));
                DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(1, 7));
            }

            // OpenBorders or TreatyDefensiveAlliance or TreatyAffiliation
            if (GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyOpenBorders) != null ||
                GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyDefensiveAlliance) != null ||
                GameContext.Current.AgreementMatrix.FindAgreement(otherCiv, foreignPower, ClauseType.TreatyAffiliation) != null)
            {
                DiplomacyHelper.ApplyTrustChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(3, 12));
                DiplomacyHelper.ApplyRegardChange(foreignPower.Counterparty, foreignPower.Owner, GetRandomNumber(2, 10));
            }
            foreignPower.DiplomacyData.Regard.UpdateAndReset();
            foreignPower.DiplomacyData.Trust.UpdateAndReset();
            foreignPower.UpdateRegardAndTrustMeters();

            //Report_CounterpartyDiplomacyData(foreignPower.CounterpartyDiplomacyData);


            _text = "Step_1175:;"
                + " _regard= " + GameEngine.Do_x_Digit_String(4, foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue.ToString())
                + ", _trust= " + GameEngine.Do_x_Digit_String(4, foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue.ToString())

                //+ " _regard= " + GameEngine.Do_x_Digit_String( 4, foreignPower.CounterpartyDiplomacyData.Regard.CurrentValue.ToString())
                //+ ", _trust= " + GameEngine.Do_x_Digit_String( 4, foreignPower.CounterpartyDiplomacyData.Trust.CurrentValue.ToString())

                + " >  AFTER Ongoing Impression: "

                + "    for " + foreignPower.Owner.Key
                + " vs " + foreignPower.Counterparty.Key
                + " > Turn " + GameContext.Current.TurnNumber
                ;
            Console.WriteLine(_text);
            //_DoOngoingRegardTrust = Environment.NewLine + _text;
            //GameLog.Client.DiplomacyDetails.DebugFormat(_text);

            // GameLog.Client.Diplomacy.DebugFormat("## _foreign_power_1 .......Owner ={0} _regard ={1} _trust ={2} After Ongoing Impression change", _foreign_power_1.Owner.Key, _foreign_power_1.DiplomacyData.Regard.CurrentValue, _foreign_power_1.DiplomacyData.Trust.CurrentValue);
        }

        //private static void Report_CounterpartyDiplomacyData(IDiplomacyData _counterpartyDiplomacyData)
        //{
        //    var _table = _counterpartyDiplomacyData;

        //    //foreach (var item in _counterpartyDiplomacyData)
        //    //{

        //    //}
        //}

        //#region methods

        public static void TrustAndRegardByTraits(ForeignPower foreignP, int impact, int similarTraits)
        {
            if (similarTraits == 10)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //    75 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 27 + impact); // before 55
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, 20 + impact); // before 40
            }
            else if (similarTraits == 6)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //    55 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 20 + impact); // before 
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, 15 + impact); // before 
            }
            else if (similarTraits == 5)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //    30 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, 10 + impact); // before 
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, 5 + impact); // before 
            }
            else if (similarTraits == 3)
            {
                //foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //      10 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, -7 + impact); // before 15
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, -10 + impact); // before 
            }
            else if (similarTraits == 0)
            {
                //    foreignP.AddRegardEvent(new RegardEvent(5, RegardEventType.TraitsInCommon,
                //        -90 + impact));
                DiplomacyHelper.ApplyTrustChange(foreignP.Counterparty, foreignP.Owner, -20 + impact);// before  95
                DiplomacyHelper.ApplyRegardChange(foreignP.Counterparty, foreignP.Owner, -15 + impact);// before 90
            }
        }
        public static void TrustAndRegardForATrait(ForeignPower foreignPow, int degree, string[] traits, string[] otherTraits)
        {
            if (traits.Contains("Warlike"))
            {
                if (otherTraits.Contains("Warlike"))
                {
                    degree = 20;
                }
                else if (otherTraits.Contains("Pleaceful"))
                {
                    degree = -25;
                }

                DiplomacyHelper.ApplyRegardChange(foreignPow.Counterparty, foreignPow.Owner, degree);
                DiplomacyHelper.ApplyTrustChange(foreignPow.Counterparty, foreignPow.Owner, degree);
            }
            else if (traits.Contains("Peaceful"))
            {
                if (otherTraits.Contains("Peaceful"))
                {
                    degree = -25;
                }
                else if (otherTraits.Contains("Warlike"))
                {
                    degree = 20;
                }

                DiplomacyHelper.ApplyRegardChange(foreignPow.Counterparty, foreignPow.Owner, degree);
                DiplomacyHelper.ApplyTrustChange(foreignPow.Counterparty, foreignPow.Owner, degree);
            }
        }
        private static readonly Random getrandom = new Random();
        //private static string _text;
        //private static bool _bool_DoSabotage;

        public static int GetRandomNumber(int min, int max)
        {
            lock (getrandom) // synchronize
            {
                return getrandom.Next(min, max);
            }
        }

        //#endregion   
    }
}
