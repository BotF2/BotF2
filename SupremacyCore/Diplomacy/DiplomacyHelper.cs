// File:DiplomacyHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Microsoft.Practices.ServiceLocation;

using Supremacy.Annotations;
using Supremacy.Client;
using Supremacy.Collections;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Supremacy.Diplomacy
{
    public static class DiplomacyHelper
    {
        private static readonly IList<Civilization> EmptyCivilizations = new Civilization[0];
        private static CollectionBase<RegardEvent> _regardEvents;
        private static Dictionary<string, bool> _acceptRejectDictionary = new Dictionary<string, bool> { { "998 vs 999", false } };
        //private static Dictionary<string, Civilization> _warPactDictionary = new Dictionary<string, Civilization> { { "987", GameContext.Current.CivilizationManagers[0].Civilization} };
        public static Civilization _diploScreenSelectedForeignPower;

        [NonSerialized]
        private static string _text;
        private static string _diploText = "";
        private static bool _bool_diplo_is_AI_controlled = GameEngine.AI_IsPlayer_AI_Controlled();

        public static Civilization DiploScreenSelectedForeignPower
        {
            get => _diploScreenSelectedForeignPower;
            set => _diploScreenSelectedForeignPower = value;
        }

        public static ForeignPowerStatus GetForeignPowerStatus([NotNull] ICivIdentity owner, [NotNull] ICivIdentity counterparty)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (counterparty == null)
            {
                throw new ArgumentNullException("counterparty");
            }

            if (owner.CivID == counterparty.CivID)
            {
                return ForeignPowerStatus.NoContact;
            }

            _regardEvents = new CollectionBase<RegardEvent>();
            return GameContext.Current.DiplomacyData[owner.CivID, counterparty.CivID].Status;
        }

        public static void ApplyGlobalTrustChange([NotNull] ICivIdentity civ, int trustDelta)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            int civId = civ.CivID;

            foreach (Diplomat diplomat in GameContext.Current.Diplomats)
            {
                if (diplomat.OwnerID == civId)
                {
                    continue;
                }

                ForeignPower foreignPower = diplomat.GetForeignPower(civ);
                if (foreignPower != null)
                {
                    _ = foreignPower.DiplomacyData.Trust.AdjustCurrent(trustDelta);
                }
            }
        }

        public static void ApplyTrustChange([NotNull] ICivIdentity civ, [NotNull] ICivIdentity otherPower, int trustDelta)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (otherPower == null)
            {
                throw new ArgumentNullException("otherPower");
            }

            Diplomat diplomat = Diplomat.Get(otherPower);
            ForeignPower foreignPower = diplomat.GetForeignPower(civ);
            //GameLog.Core.Diplomacy.DebugFormat("BEFORE: _civ1 = {0}, otherPower.CivID = {1}, trustDelta = {2}, _diplomat.Owner = {3}, foreignPower.OwnerID =n/v, CurrentTrust =n/v",
            //_civ1, otherPower.CivID, trustDelta, _diplomat.Owner);

            //GameLog.Core.Diplomacy.DebugFormat(
            //    "BEFORE: _civ1 = {0}, otherPower = {1}, trustDelta = {2}, _diplomat.Owner = {3}, foreignPower = {4}, CurrentTrust = {5}",
            //    GameContext.Current.CivilizationManagers[_civ1.CivID].Civilization.ShortName,
            //    GameContext.Current.CivilizationManagers[otherPower.CivID].Civilization.ShortName,
            //    trustDelta, _diplomat.Owner,
            //    GameContext.Current.CivilizationManagers[foreignPower.OwnerID].Civilization.ShortName,
            //    foreignPower.DiplomacyData.Trust.CurrentValue);

            if (foreignPower != null)
            {
                _ = foreignPower.DiplomacyData.Trust.AdjustCurrent(trustDelta);
                foreignPower.DiplomacyData.Trust.UpdateAndReset();
                foreignPower.UpdateRegardAndTrustMeters();
            }

            //GameLog.Core.Diplomacy.DebugFormat(
            //    "AFTER : _civ1 = {0}, otherPower = {1}, trustDelta = {2}, _diplomat.Owner = {3}, foreignPower = {4}, CurrentTrust = {5}",
            //    GameContext.Current.CivilizationManagers[_civ1.CivID].Civilization.ShortName,
            //    GameContext.Current.CivilizationManagers[otherPower.CivID].Civilization.ShortName,
            //    trustDelta, _diplomat.Owner,
            //    GameContext.Current.CivilizationManagers[foreignPower.OwnerID].Civilization.ShortName,
            //    foreignPower.DiplomacyData.Trust.CurrentValue);
        }
        public static void ApplyRegardChange([NotNull] ICivIdentity civ, [NotNull] ICivIdentity otherPower, int regardDelta)
        {
            if (civ == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (otherPower == null)
            {
                throw new ArgumentNullException("otherPower");
            }

            Diplomat diplomat = Diplomat.Get(otherPower);
            ForeignPower foreignPower = diplomat.GetForeignPower(civ);

            // GameLog.Core.Diplomacy.DebugFormat(Environment.NewLine + "   Turn {6};BEFORE: otherPower.CivID=;{1};foreignPower.OwnerID=;{4};regardDelta=;{2};CurrentTrust=;{5};_diplomat.Owner=;{3};_civ1=;{0}" + Environment.NewLine,
            // _civ1, otherPower.CivID, regardDelta, _diplomat.Owner, foreignPower.OwnerID, foreignPower.DiplomacyData.Trust.CurrentValue, GameContext.Current.TurnNumber);

            if (foreignPower != null)
            {
                foreignPower.DiplomacyData.Regard.AdjustCurrent(regardDelta);
                foreignPower.DiplomacyData.Regard.UpdateAndReset();
                foreignPower.UpdateRegardAndTrustMeters();

            }
            // GameLog.Core.Diplomacy.DebugFormat(Environment.NewLine + "   Turn {6};AFTER : otherPower.CivID=;{1};foreignPower.OwnerID=;{4};regardDelta=;{2};CurrentTrust=;{5};_diplomat.Owner=;{3};_civ1=;{0}" + Environment.NewLine,
            //_civ1, otherPower.CivID, regardDelta, _diplomat.Owner, foreignPower.OwnerID, foreignPower.DiplomacyData.Trust.CurrentValue, GameContext.Current.TurnNumber);
        }
        public static void ApplyRegardDecay(RegardEventCategories category, RegardDecay decay)
        {
            for (int i = 0; i < _regardEvents.Count; i++)
            {
                RegardEvent regardEvent = _regardEvents[i];

                // Regard events with a fixed duration do not decay.
                if (regardEvent.Duration > 0)
                {
                    continue;
                }

                int regard = regardEvent.Regard;
                if (regard == 0)
                {
                    _regardEvents.RemoveAt(i--);
                    continue;
                }

                if (!regardEvent.Type.GetCategories().HasFlag(category))
                {
                    continue;
                }

                regard = regard > 0 ? Math.Max(0, (int)(regard * decay.Positive)) : Math.Min(0, (int)(regard * decay.Negative));

                if (regard == 0)
                {
                    _regardEvents.RemoveAt(i--);
                }
                else
                {
                    regardEvent.Regard = regard;
                }
            }
        }

        public static Colony GetSeatOfGovernment([NotNull] Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            Diplomat diplomat = GameContext.Current.Diplomats[who.CivID];
            if (diplomat == null)
            {
                return null;
            }

            return diplomat.SeatOfGovernment;
        }

        public static void SendWarDeclaration([NotNull] Civilization _civ1, [NotNull] Civilization _civ2, Tone tone = Tone.Calm)
        {
            GameLog.Client.Diplomacy.DebugFormat("************** Diplo: SendWarDeclaration...");
            if (_civ1 == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (_civ2 == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            if (_civ1 == _civ2)
            {
                GameLog.Core.Diplomacy.ErrorFormat(
                    "Civilization {0} attempted to declare war on itself.",
                    _civ1.ShortName);

                return;
            }

            if (Status_AtWar(_civ1, _civ2))
            {
                GameLog.Core.Diplomacy.WarnFormat(
                    "Civilization {0} attempted to declare war on {1}, but they were already at war.",
                    _civ1.ShortName,
                    _civ2.ShortName);

                return;
            }

            Diplomat diplomat = Diplomat.Get(_civ1);
            ForeignPower foreignPower = diplomat.GetForeignPower(_civ2);

            Statement proposal = new Statement(_civ1, _civ2, StatementType.WarDeclaration, tone);

            foreignPower.StatementSent = proposal;
            _text = "Step_2312:; WarDeclaration (StatementSent) from " + _civ1 + " to " + _civ2;
            Console.WriteLine(_text);
            //GameLog.Client.Diplomacy.DebugFormat(_text);


            foreignPower.CounterpartyForeignPower.StatementReceived = proposal;
            _text = "Step_2313:; WarDeclaration (StatementReceived) to " + _civ2 + " from  " + _civ1;
            Console.WriteLine(_text);
            //GameLog.Client.Diplomacy.DebugFormat("************** Diplo: SendWarDeclaration turned to RECEIVED at ForeignPower...");
        }

        public static void SpecificCivAcceptingRejecting([NotNull] StatementType statementType) // read statment type to get civIDs and bool accpet reject
        {
            string statementAsString = GetEnumString(statementType);
            string otherCivID = statementAsString.Substring(1, 1);
            string aCivID = statementAsString.Substring(2, 1);
            string trueFalse = statementAsString.Substring(0, 1);
            int aCivint = int.Parse(aCivID);
            int otherCivint = int.Parse(otherCivID);
            Civilization aCiv = GameContext.Current.Civilizations[aCivint];
            Civilization otherCiv = GameContext.Current.Civilizations[otherCivint];
            Diplomat diplomat = Diplomat.Get(aCiv);
            ForeignPower foreignPower = diplomat.GetForeignPower(otherCiv);
            bool accepting = false;
            if (trueFalse == "T")
            {
                accepting = true;
            }

            if (accepting)
            {
                if (foreignPower.CounterpartyForeignPower.LastProposalSent != null) // _civ1 is owner of the foreignpower looking for a ProposalRecieved
                {
                    _ = AcceptProposalVisitor.Visit(foreignPower.CounterpartyForeignPower.LastProposalSent);
                    CivilizationManagerMap civManagers = GameContext.Current.CivilizationManagers;
                    Civilization civ1 = foreignPower.CounterpartyForeignPower.Owner;
                    Civilization civ2 = foreignPower.Owner;



                    civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, foreignPower.ResponseSent));

                    civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, foreignPower.ResponseSent));

                    foreignPower.CounterpartyForeignPower.LastProposalSent = null;
                    foreignPower.ResponseSent = null;
                }
            }
            else
            {
                if (foreignPower.CounterpartyForeignPower.LastProposalSent != null) // _civ1 is owner of the foreignpower looking for a ProposalRecieved
                {
                    RejectProposalVisitor.Visit(foreignPower.CounterpartyForeignPower.LastProposalSent);
                    CivilizationManagerMap civManagers = GameContext.Current.CivilizationManagers;
                    Civilization civ1 = foreignPower.CounterpartyForeignPower.Owner;
                    Civilization civ2 = foreignPower.Owner;

                    civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, foreignPower.ResponseSent));

                    civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, foreignPower.ResponseSent));

                    foreignPower.CounterpartyForeignPower.LastProposalSent = null;
                    foreignPower.ResponseSent = null;
                }
            }
        }

        // find entry in dictionary and send as foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal; or Reject
        //public static void AcceptingRejecting([NotNull] ICivIdentity _civ)
        public static void AcceptingRejecting([NotNull] Civilization _civ1)

        {

            _text = "Step_1371:; AcceptingRejecting... for " + _civ1;
            Console.WriteLine(_text);

            if (_civ1 == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            //bool _bool_diplo_is_AI_controlled = false;

            if (_civ1.IsHuman)
            {
                _bool_diplo_is_AI_controlled = GameEngine.AI_IsPlayer_AI_Controlled();

                _text = /*_newline +*/ "Step_1113:; _diplo_is_AI_controlled= "
                        + " * > AIcontrolled= " + _bool_diplo_is_AI_controlled
                        + " for " + _civ1
                        ;
                Console.WriteLine(_text);
            }

            //Civilization _civ1 = (Civilization)_civ1;
            Diplomat _diplomat = Diplomat.Get(_civ1);

            foreach (Civilization _civ_2 in GameContext.Current.Civilizations)
            {
                if (_civ1 == _civ_2)
                {
                    continue;
                }

                //if (!_civ_2.IsEmpire)  // do it as well for minors
                //{
                //    continue;
                //}

                ForeignPower foreignPower = _diplomat.GetForeignPower(_civ_2);

                bool accepting = false;

                if (foreignPower.StatementReceived != null)
                {
                    Diplomacy_4_Statement_Received(foreignPower.StatementReceived);
                }



                int _random = RandomHelper.Random(2);

                if (_random == 1)
                {
                    accepting = true;


                }

                string powerID = foreignPower.OwnerID.ToString() + " vs " + foreignPower.CounterpartyID.ToString();

                _text = "Step_7444:; DiplomacyHelper.cs > AcceptingRejecting"
                    + " > OwnerID= " + GameEngine.Do_x_Digit_String(3, foreignPower.OwnerID.ToString())
                    + " > CounterpartyID= " + GameEngine.Do_x_Digit_String(3, foreignPower.CounterpartyID.ToString())
                    + " > PowerID(inDict)= >>   " + powerID.ToString()
                    ;
                Console.WriteLine(_text);
                //GameLog.Client.Diplomacy.DebugFormat(_text);

                // AcceptRejectDictionary
                //if (_acceptRejectDictionary.ContainsKey(powerID)) // check dictionary with key for bool value to accept reject
                //{
                //    //GameLog.Client.Diplomacy.DebugFormat("Found it in Dictionary");
                //    accepting = _acceptRejectDictionary[powerID];
                if (accepting)
                {
                    if (foreignPower.ProposalReceived != null) // _civ1 is owner of the foreignpower looking for a ProposalRecieved
                    {
                        foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;

                        _text = "Step_1372:; "
                            + "PendingAction: ACCEPT = " + foreignPower.PendingAction.ToString()
                            + ", Counterparty= " + foreignPower.Counterparty.ShortName
                            + ", Onwer= " + foreignPower.Owner.ShortName

                            ;
                        _diploText += _text;
                        Console.WriteLine(_text);
                        //GameLog.Client.Diplomacy.DebugFormat(_text);

                        //////if (foreignPower.ProposalReceived != null)
                        //////    GameLog.Client.Diplomacy.DebugFormat(
                        //////       "## ProposlaReceived count={0},  = {1} LastProposalReceived= {2}"
                        //////       , foreignPower.ProposalReceived.Clauses.Count()
                        //////       , foreignPower.LastProposalReceived.Clauses.Count()
                        //////       , foreignPower.Owner.ShortName);
                        foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                        foreignPower.ProposalReceived = null;
                        //////GameLog.Client.Diplomacy.DebugFormat("LastProposalReceived ={0} on foreignPower.Owner ={1} clause count ={2}"
                        //////    , foreignPower.LastProposalReceived.ToString()
                        //////    , foreignPower.LastProposalReceived.Clauses.Count()
                        //////    );
                    }
                }
                else // Rejecting
                {
                    if (foreignPower.ProposalReceived != null)
                    {
                        foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;

                        _text = "Step_1374:; "
                            + "PendingAction: REJECT = " + foreignPower.PendingAction.ToString()
                            + ", Counterparty= " + foreignPower.Counterparty.ShortName
                            + ", Owner= " + foreignPower.Owner.ShortName

                            ;
                        _diploText += _text;
                        Console.WriteLine(_text);
                        //                //GameLog.Client.Diplomacy.DebugFormat(_text);

                        foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                        foreignPower.ProposalReceived = null;
                        //            }

                    }

                }
            }
        }



        public static void Diplomacy_0_DoDiplomacy()
        {
            //string _text = "";
            string _sender_civ = "";
            string _recipient_civ = "";


            // FIRST: Pending Actions
            foreach (Civilization _civ1 in GameContext.Current.Civilizations)
            {
                _text = "\r\nStep_1351:; DoDiplomacy for > " + _civ1
                    + ",       _bool_diplo_is_AI_controlled= " + _bool_diplo_is_AI_controlled;
                //string _newline = Environment.NewLine;
                Console.WriteLine(_text);

                string _diploStatusText = ""; _diploStatusText += " " + _diploStatusText; // dummy - please keep

                CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];
                Diplomat _diplomat_civ1 = Diplomat.Get(_civ1);
                _civM_1.Assault_Value_Defense_and_Distance = 999993;

                if (!_civ1.IsHuman)
                {
                    DiplomacyHelper.AcceptingRejecting(_civ1);
                }

                Report_Regard_and_Trust(_civ1, _civM_1);

                DiplomacyHelper.Diplomacy_1_Basics(_civ1, _civM_1);  // e.g. Status = NoContact, War etc...


                // Second: Schedule delivery of outbound messages  Including Statementreceived
                _text = "Step_3091:; NEXT: *Second* Outgoing";
                //if (_combatWriteDirectly)
                //    //Console.WriteLine(_text);
                //GameLog.Core.DiplomacyDetails.DebugFormat(_text);



                foreach (Civilization _civ2 in GameContext.Current.Civilizations)
                {
                    if (_civ1 == _civ2) { continue; }

                    ForeignPower _civ2_foreign_power = _diplomat_civ1.GetForeignPower(_civ2);
                    CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];

                    if (_civ2_foreign_power.StatementReceived == null
                        && _civ2_foreign_power.ProposalSent == null
                        && _civ2_foreign_power.StatementSent == null
                        && _civ2_foreign_power.ResponseSent == null
                        )
                    {
                        continue;
                    }

                    //string _civ_pair = _civ1.Key + "-" + _civ2.Key;

                    _text = "\r\nStep_3192:; " + DateTime.Now
                        + " > Diplomacy now > " + _civ1 + " vs " + _civ2;
                    Console.WriteLine(_text);


                    if (_civ2_foreign_power.ProposalSent != null)
                    {
                        Diplomacy.DiplomacyHelper.Diplomacy_5_Proposal_Sent(_civ2_foreign_power.ProposalSent);//  Second.2 = proposalSent
                                                                                                         //_sender_civ = _civ2_foreign_power.ProposalSent.Sender.ToString();
                                                                                                         //_recipient_civ = _civ2_foreign_power.ProposalSent.Recipient.ToString();

                        //_text = "Step_7555:; "
                        //    + DateTime.Now
                        //    + " > _sender_civ= " + _sender_civ
                        //    + " > " 
                        //    + ", _civ1.LongName= " + _civ1.LongName
                        //    + " > must be identical !!" 

                        //    ;
                        //Console.WriteLine(_text);


                        //if (_sender_civ == _civ1.LongName)
                        //{
                        //Diplomacy.DiplomacyHelper.Diplomacy_5_Proposal_Sent(_civ2_foreign_power.ProposalSent);//  Second.2 = proposalSent
                        //}

                    }

                    if (_civ2_foreign_power.StatementSent != null)
                    {
                        _sender_civ = _civ2_foreign_power.StatementSent.Sender.ToString();
                        _recipient_civ = _civ2_foreign_power.StatementSent.Recipient.ToString();

                        //if (_sender_civ == _civ1.Key)
                        //{
                        Diplomacy.DiplomacyHelper.Diplomacy_6_Statement_Sent(_civ2_foreign_power.StatementSent);//  Second.3 = statementSent
                                                                                                           //}
                    }

                    if (_civ2_foreign_power.ResponseSent != null)
                    {
                        _sender_civ = _civ2_foreign_power.ResponseSent.Sender.ToString();
                        _recipient_civ = _civ2_foreign_power.ResponseSent.Recipient.ToString();

                        //if (_sender_civ == _civ1.Key)
                        //{
                        Diplomacy.DiplomacyHelper.Diplomacy_7_Response_Sent(_civ2_foreign_power.ResponseSent);//  Second.4 = responseSent
                                                                                                         //}
                    }
                    //_civM_1.Target_CivList.Add(_civ2);
                }
                //}


                // Third: Fulfill agreement obligations
                foreach (IAgreement agreement in GameContext.Current.AgreementMatrix)
                {
                    AgreementFulfillmentVisitor.Visit(agreement);
                }


                //_civM_1.Target_CivList.Add(_civ2);

                //if (_civM_1.Assault_TargetCiv == null)
                //{

                //if (_civ1.IsHuman)
                //{
                //    //Debugger.Break();
                //    goto For_Human_Players_no_AI_Assault_Locations;
                //}

                ////Assault_Accumulate_Location_1_Handle(_civ1, _civM_1);

                //if (_civM_1.Assault_Location != null
                //    && _civM_1.Assault_Accumulate_Location_1.ToString() != "(0, 0)")
                //{
                //    Civilization _civ2 = GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv.CivID].Civilization;
                //    //_text += "SystemAssault Location 1 = " + _civM_1.Assault_Accumulate_Location_1 + ", ";
                //    _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1
                //        , "Assault_Location= " + _civM_1.Assault_Location, "", "", SitRepPriority.Purple));
                //    // no center on Target
                //    //_civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv.CivID].HomeSystem.Location
                //    //    , "SystemAssault TargetCiv = " + _civM_1.Assault_TargetCiv, "", "", SitRepPriority.Purple));

                //    //Diplomat _foreignPowerCiv2 = Diplomat.Get(_civ2);
                //    ForeignPower _foreignPowerCiv2 = _diplomat_civ1.GetForeignPower(_civ2);
                //    ForeignPowerStatus _foreignPowerStatus = _diplomat_civ1.GetForeignPower(_civ2).DiplomacyData.Status;

                //    if (_foreignPowerStatus != ForeignPowerStatus.AtWar)
                //    {

                //        // 
                //        CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv.CivID];
                //        UnitAI.GetBestSystemFor_AccumulateFor_SystemAttack(_civM_2.HomeSystem.Sector, _civM_1.HomeSystem.Sector, out Sector _sector);
                //        _civM_1.Assault_Accumulate_Sector_1 = _sector;

                //        string _systemAssaultSector_Text = "Assault_Accumulate_Sector_1 is set to Sector " + _sector.ToString();
                //        _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _sector.Location, _systemAssaultSector_Text, "", "", SitRepPriority.Purple));

                //        string _systemAssaultLocation_Text = "Assault_Accumulate_Location_1 is set to Location " + _sector.Location.ToString();
                //        _civM_1.SitRepEntries.Add(new ReportEntry_CoS(_civ1, _sector.Location, _systemAssaultLocation_Text, "", "", SitRepPriority.Purple));

                //        _text = "DefenseValueSector(_civM_2.HomeSystem.Sector.TradeRouteIndicator);";

                //        //int _defense = 10; // 
                //        _civM_1.Assault_DefenseValue = _civM_2.HomeSystem.Sector.GetDefenseValueSector();
                //        //_civM_2.HomeSystem.Sector.GetDefenseValueSector();
                //        // DefenseValue;//
                //        //GetDefenseValueSector(_civM_2.HomeSystem.Sector);

                //        //if (_civM_2.HomeSystem.Sector.Station != null)
                //        //{
                //        //    _defense += _civM_2.HomeSystem.Sector.Station.Fire_Power_Orbital / 200;  // station only half
                //        //    _defense += _civM_2.HomeSystem.Colony.Population.CurrentValue;  // 
                //        //    if (_civM_2.HomeSystem.Colony.OrbitalBatteries.Count > 0)
                //        //    {
                //        //        _defense += _civM_2.HomeSystem.Colony.OrbitalBatteries.Count
                //        //                         * (_civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Count
                //        //                         * _civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.PrimaryWeapon.Damage)
                //        //                         + (_civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Count
                //        //                         * _civM_2.HomeSystem.Colony.OrbitalBatteries[0].Design.SecondaryWeapon.Damage)

                //        //                         ; 


                //        //    }
                //        //    _civM_1.Assault_DefenseValue = _defense;
                //        //}

                //    }


                //}
                //else
                //{
                //    if (_civM_1.Assault_TargetCiv != null)
                //    {
                //        string _targetCivtext = "Assault_TargetCiv is set to= " + _civM_1.Assault_TargetCiv.Name
                //            + " at " + _civM_1.HomeSystem.Location
                //            ;
                //        _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _targetCivtext, "", "", SitRepPriority.Gray));

                //        if (_civM_1.Assault_Accumulate_Sector_1 != null)
                //        {
                //            CivilizationManager _civM_temp = GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv];
                //            UnitAI.GetBestSystemFor_AccumulateFor_SystemAttack(_civM_temp.HomeSystem.Sector, _civM_1.HomeSystem.Sector, out Sector _sector);
                //            _civM_1.Assault_Accumulate_Sector_1 = _sector;
                //            string _systemAssaultSector_Text = "Assault_Accumulate_Sector_1 is set to Sector " + _sector.ToString();
                //            _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _systemAssaultSector_Text, "", "", SitRepPriority.Purple));

                //            string _systemAssaultLocation_Text = "Assault_Accumulate_Location_1 is set to Location " + _sector.Location.ToString();
                //            _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _systemAssaultLocation_Text, "", "", SitRepPriority.Purple));
                //        }
                //        else
                //        {

                //            string _noTargetCivtext = "Not set: Assault_Accumulate_Location_1";
                //            _civM_1.SitRepEntries.Add(new ReportEntry_NoAction(_civ1, _noTargetCivtext, "", "", SitRepPriority.Gray));

                //        }
                //    }
                //} // end of else


                //if (_writeDirectly) Console.WriteLine("Step_7727:; _diplomacyBasicsSummary_Text="
                //    + _diplomacyBasicsSummary_Text + _newline
                //    + "End of _diplomacyBasicsSummary_Text" + _newline

                //    );

                //_diplomacyBasicsSummary_Text = "";

                if (_civM_1.Assault_TargetCiv != null)
                {
                    _text = "Step_7729:; Assault_TargetCiv= " + _civM_1.Assault_TargetCiv.Name
                            + " at " + _civM_1.HomeSystem.Location
                            + " for " + _civM_1.Civilization.Key
                            ;
                    //if (_writeDirectly)
                    Console.WriteLine(_text);
                }
                //For_Human_Players_no_AI_Assault_Locations:;


            } // End of foreach _civ1
            Report_Agreement_Matrix();
        }

        private static void Report_Regard_and_Trust(Civilization _civ1, CivilizationManager _civM_1)
        {
            foreach (Civilization _civ2 in GameContext.Current.Civilizations)
            {

                if (_civ1 == _civ2)
                {
                    continue;
                }

                Diplomat _diplomat1 = Diplomat.Get(_civ1);
                //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];

                ForeignPower _diplomatForeignPower_Civ2 = _diplomat1.GetForeignPower(_civ2);
                CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];

                var _diplomacyData = GameContext.Current.DiplomacyData[_civ1, _civ2];

                ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

                if (_foreignPowerStatus == ForeignPowerStatus.NoContact)
                {
                    continue;
                }
                // SitRep for all
                if (_foreignPowerStatus != ForeignPowerStatus.OwnerIsSubjugated)
                {
                    string _diplomat1_Location_String = "( Empire )";
                    if (!_diplomat1.Owner.IsEmpire && _diplomat1.SeatOfGovernment != null)
                        _diplomat1_Location_String = _diplomat1.SeatOfGovernment.Location.ToString();

                    _text = "Relation > "
                        + "Regard: " + GameEngine.Do_x_Digit_String(4, _diplomacyData.Regard.CurrentValue.ToString())
                        + ", Trust: " + GameEngine.Do_x_Digit_String(4, _diplomacyData.Trust.CurrentValue.ToString())

                        + " > " + _foreignPowerStatus

                        + " vs " + _diplomat1.Owner
                        + " " + _diplomat1_Location_String
                        ;
                    // too much info
                    //Console.WriteLine("Step_7429:; " + _text + "; Turn " + GameContext.Current.TurnNumber + ";SR for " + _civ2.Name);

                    GameContext.Current.CivilizationManagers[_civ2].SitRepEntries.Add(
                        new ReportEntry_ShowDiplo(_civ2, _text, "", "", SitRepPriority.GreenDark2));

                }
            }
        }

        public static void Report_Agreement_Matrix()
        {
            string _text = "";
            string _agreement_total_text = Environment.NewLine
                + "Step_2278:; Report_Agreement_Matrix" + Environment.NewLine;
            foreach (IAgreement _agreement in GameContext.Current.AgreementMatrix)
            {
                //AgreementFulfillmentVisitor.Visit(_agreement);
                _agreement_total_text +=
                    /*+ " for " + */_agreement.Proposal.Clauses[0].ClauseType.ToString()
                    + ", Start-Turn= " + _agreement.StartTurn
                    + ", End= " + _agreement.EndTurn

                    + ", Sender= " + _agreement.Sender
                    + " to " + _agreement.Recipient
                    //+ ", DATA= " + _agreement.Data

                    ;
                //Console.WriteLine(_agreement_total_text);
                //Debugger.Break();
            }
            Console.WriteLine(_agreement_total_text);
            //Debugger.Break();
        }

        public static void Diplomacy_1_Basics(Civilization _civ1, CivilizationManager _civM_1)
        {
            int _targetDistance = 99;
            _civM_1.Assault_Value_Defense_and_Distance = 9999997;

            string _text;
            string _diplomacyBasicsSummary_Text = "";
            //string _newline = Environment.NewLine;

            bool _writeDirectly = true;
            bool _player_is_human = GameEngine.IsCivM_Human_Player(_civM_1);

            AgreementMatrix agreementMatrix = GameContext.Current.AgreementMatrix;

            //Dictionary<Civilization, int> _possibleTargetCivs = new Dictionary<Civilization, int >(); // for Assault or better SystemAssault
            List<Civilization> _possibleTargetCivs = new List<Civilization>(); // for Assault or better SystemAssault

            foreach (Civilization _civ2 in GameContext.Current.Civilizations)
            {

                if (_civ1 == _civ2)
                {
                    continue;
                }

                Diplomat _diplomat1 = Diplomat.Get(_civ1);
                //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];

                ForeignPower _diplomatForeignPower_Civ2 = _diplomat1.GetForeignPower(_civ2);
                CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];

                ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

                if (_foreignPowerStatus == ForeignPowerStatus.NoContact)
                {
                    continue;
                }

                //_diplomat1.GetForeignPower(_civ2).CounterpartyForeignPower.

                int _regard = _diplomatForeignPower_Civ2.DiplomacyData.Regard.CurrentValue;
                int _trust = _diplomatForeignPower_Civ2.DiplomacyData.Trust.CurrentValue;

                int _random = RandomHelper.Random(2);

                //if (_foreignPowerStatus != ForeignPowerStatus.NoContact)
                //{
                _text = string.Concat("\r\nStep_7731:; Do_13_Diplomacy >> Diplomacy_1_Basics >>>>> "
                    , _civ1
                    , " ; to ; ", _civ2
                    , " ; * ", _foreignPowerStatus

                    , " * ; R= ", _regard
                    , " ; T= ", _trust
                    , " ; random= ", _random
                    );

                if (_writeDirectly)
                    Console.WriteLine(_text);
                _diplomacyBasicsSummary_Text += Environment.NewLine + _text;





                // Find Assault_TargetCiv
                if (_player_is_human)
                {
                    //Debugger.Break();
                }

                _possibleTargetCivs.Add(_civ2);
                _possibleTargetCivs = _possibleTargetCivs.Distinct().ToList();



                _text = "RegardLevels"
                    + "100 > TotalWar >  (Declare War)"
                    + "300 > ColdWar >  Hostile"
                    + "400 >         > Cold "
                    + "450 > Neutral >  Open Borders"
                    + "500 >       >    Peace"
                    + "600 > Friend >  Defence Alliance (MAJOR only)"
                    + "700 >      >    Affiliated (MAJOR only)"
                    + "800 > Allied >  Full Alliance (MAJOR only)"
                    + "900 >        >  (Membership) (Minor only)"
                    + "1000 > Unified > "
                    ;

                _text = "if xy than offer treaty ... or is this done somewhere else";
                _text = "" +
                    "NoContact = 0," +
                    "OwnerIsSubjugated," +
                    "CounterpartyIsSubjugated," +
                    "AtWar,+" +
                    "Hostile,+" +
                    "Cold,+" +
                    "Neutral,+" +
                    "Peace,+" +
                    "Friendly,+" +
                    "Affiliated,+" +
                    "OwnerIsMember,+" +
                    "CounterpartyIsMember,+" +
                    "Allied,+" +
                    "Self,+" +
                    "OwnerIsUnreachable,+ " +
                    "CounterpartyIsUnreachable";



                //_text = "Step_7742:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 
                //    + "; > Regard =;" + _regard + "; > Trust =;" + _trust;
                //if (_writeDirectly) Console.WriteLine(_text);
                //////_text = "Step_7744:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Trust =;" + _trust;
                //////if (_writeDirectly) Console.WriteLine(_text);

                if (_foreignPowerStatus == ForeignPowerStatus.Affiliated)
                {
                    //_text = "Step_7750:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Affiliated";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _text = "IMPACT";
                    if (_regard < 850)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3); // 2 each turnnumber
                    if (_trust < 800)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 4);

                    _text = "OFFER_2";
                    if (!_civ1.IsHuman && !_civ2.IsEmpire && _regard > 800 && _random == 1)
                    {
                        List<Clause> _clauses = new List<Clause>();

                        if (!_civ1.IsEmpire && !agreementMatrix.IsAgreementActive(_civ1, _civ2, ClauseType.TreatyFullAlliance))
                        {
                            _clauses.Add(new Clause(ClauseType.TreatyFullAlliance));
                            var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                            if (_newProposal != null)
                            {
                                var _sendOrder = new SendProposalOrder(_newProposal);
                                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                            }
                        }
                    }

                }

                if (_foreignPowerStatus == ForeignPowerStatus.Allied)
                {
                    //_text = "Step_7760:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Allied";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _text = "IMPACT";
                    if (_regard < 850)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 800)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3);

                    _text = "OFFER_1";
                    if (!_civ1.IsHuman && _regard > 800 && _random == 1)
                    {
                        List<Clause> _clauses = new List<Clause>();

                        if (!_civ1.IsEmpire && !agreementMatrix.IsAgreementActive(_civ1, _civ2, ClauseType.TreatyFullAlliance))
                        {
                            _clauses.Add(new Clause(ClauseType.TreatyFullAlliance));
                            var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                            if (_newProposal != null)
                            {
                                var _sendOrder = new SendProposalOrder(_newProposal);
                                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                            }
                        }
                    }

                }

                if (_foreignPowerStatus == ForeignPowerStatus.Friendly)  // Open Borders
                {
                    //_text = "Step_7770:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Friendly";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _text = "IMPACT";
                    if (_regard < 650)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 600)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2);


                    _text = "OFFER";
                    if (!_civ1.IsHuman && _regard > 600 && _random == 1)
                    {
                        List<Clause> _clauses = new List<Clause>();

                        if (!_civ1.IsEmpire && !agreementMatrix.IsAgreementActive(_civ1, _civ2, ClauseType.TreatyDefensiveAlliance))
                        {
                            _clauses.Add(new Clause(ClauseType.TreatyDefensiveAlliance));
                            var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                            if (_newProposal != null)
                            {
                                var _sendOrder = new SendProposalOrder(_newProposal);
                                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                            }
                        }
                    }

                    _text = "OFFER_2";
                    if (!_civ1.IsHuman && _regard > 700 && _random == 1)
                    {
                        List<Clause> _clauses = new List<Clause>();

                        if (!_civ1.IsEmpire && !agreementMatrix.IsAgreementActive(_civ1, _civ2, ClauseType.TreatyAffiliation))
                        {
                            _clauses.Add(new Clause(ClauseType.TreatyAffiliation));
                            var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                            if (_newProposal != null)
                            {
                                var _sendOrder = new SendProposalOrder(_newProposal);
                                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                            }
                        }
                    }

                }


                if (_foreignPowerStatus == ForeignPowerStatus.Peace) // > 500
                {
                    //_text = "Step_7780:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Peace";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    //if (_regard < 850)
                    //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    _text = "IMPACT";
                    if (_trust < 600)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 3);

                    _text = "OFFER";
                    if (!_civ1.IsHuman && _regard > 500 && _random == 1)
                    {
                        List<Clause> _clauses = new List<Clause>();

                        if (!agreementMatrix.IsAgreementActive(_civ1, _civ2, ClauseType.TreatyOpenBorders))
                        {
                            _clauses.Add(new Clause(ClauseType.TreatyOpenBorders));
                            var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                            if (_newProposal != null)
                            {
                                var _sendOrder = new SendProposalOrder(_newProposal);
                                ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                            }
                        }
                    }

                }

                if (_foreignPowerStatus == ForeignPowerStatus.Neutral)
                {
                    //_text = "Step_7710:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Neutral";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _text = "IMPACT";
                    if (_regard < 650)
                        DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2); // 2 each turnnumber
                    if (_trust < 600)
                        DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, 2);

                    _text = "OFFER";
                    if (!_civ1.IsHuman && _regard > 450 && _random == 1)
                    {
                        List<Clause> _clauses = new List<Clause>();
                        _clauses.Add(new Clause(ClauseType.TreatyOpenBorders));
                        var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                        if (_newProposal != null)
                        {
                            var _sendOrder = new SendProposalOrder(_newProposal);
                            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                        }
                    }
                }

                //Debugger.Break()
                //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }


                //AtWar
                if (_foreignPowerStatus == ForeignPowerStatus.AtWar)
                {
                    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                    //_text = "Step_7732:; Do_13_Diplomacy > " + _civ1 + "; vs ; " + _civ2 + "; > AtWar";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    //_diplomacyBasicsSummary_Text += Environment.NewLine + _text;

                    List<Clause> _clauses = new List<Clause>();

                    int _random_end_war = RandomHelper.Random(2);

                    int _turns_ago = GameContext.Current.TurnNumber - _diplomatForeignPower_Civ2.DiplomacyData.LastStatusChange;
                    _text = "Step_9444:; " + _turns_ago + " turns ago" + " was DiplomacyData.LastStatusChange, _random= " + _random_end_war;
                    Console.WriteLine(_text);



                    if (_random_end_war == 1
                        //&& _random_end_war == 1
                        // nonsense >> && !agreementMatrix.IsAgreementActive(_civ1, _civ2, ClauseType.)
                        && _turns_ago > 1
                        )
                    {
                        _clauses.Add(new Clause(ClauseType.TreatyCeaseFire));
                        var _newProposal = new NewProposal(_civ1, _civ2, _clauses);
                        if (_newProposal != null)
                        {
                            _text = "Step_9451:; Proposal CeaseFire from " + _civ1
                                + " to " + _civ2
                                ;
                            Console.WriteLine(_text);

                            var _sendOrder = new SendProposalOrder(_newProposal);
                            ServiceLocator.Current.GetInstance<IPlayerOrderService>().AddOrder(_sendOrder);
                        }
                    }

                    //List<Civilization> _possibleTargetCivs = new List<Civilization>();


                    //// Find Assault_TargetCiv
                    //if (_civ1.IsHuman)
                    //{
                    //    //Debugger.Break();
                    //}

                    //_possibleTargetCivs.Add(_civ2);
                    //_possibleTargetCivs.Distinct();

                }

                //Find_Assault_Targets(_civ1, _civM_1);

                if (_player_is_human)
                {
                    //Debugger.Break(); // see below
                }

                // Find new TargetCiv
                List<Civilization> _target_help_list = new List<Civilization>() { _civ1 };

                //_target_help_list.Add(_civ1);
                if (_civM_1.Target_CivList == null
                    || _civM_1.Target_CivList.Count == 0)
                {
                    _civM_1.Target_CivList = _target_help_list;
                }
                else
                {
                    //_civM_1.Target_CivList.AddRange(_possibleTargetCivs);
                    _target_help_list.AddRange(_possibleTargetCivs);

                    //_target_help_list = _civM_1.Target_CivList;
                    //_target_help_list = _target_help_list;
                    _civM_1.Target_CivList = _target_help_list.Distinct().ToList();

                    //Debugger.Break()
                    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }

                }


                //if (_possibleTargetCivs.Count > 0)
                //{
                //    //_civM_1.Target_CivList = _possibleTargetCivs;
                //    _civM_1.Target_CivList.AddRange(_possibleTargetCivs);
                //}


                //_civM_1.TargetList_Update(_target_help_list);
                //_civM_1.TargetList_Update(_civ1);

                //Array<Civilization, int distance, int defense_value> _target_ColoniesLocations = new Array<Civilization, int, int>(); // find out the nearest one

                //Dictionary<int, int> ColonyTargetValues = new Dictionary<int, int>(); // find out the nearest one
                //ColonyTargetValues.Add(99, 999999);  // avoid an empty Dictionary

                Dictionary<MapLocation, ColonyTargetValues> _target_ColoniesLocations = new Dictionary<MapLocation, ColonyTargetValues>(); // find out the nearest one

                //var _colonyTargetValues = new ColonyTargetValues(99, 999999);
                //_target_ColoniesLocations.Add(_civM_1.HomeColony, 99 ,999999);  // avoid an empty Dictionary

                _target_ColoniesLocations.Add(_civM_1.HomeColony.Location, new ColonyTargetValues(99, 999999));
                //_target_ColoniesLocations[colonyB] = new ColonyTargetValues(200, 60);
                int _lowest_targetDistance = 99;

                MapLocation _new_assault_location = new MapLocation();
                _civM_1.Assault_Value_Defense_and_Distance = 999996;
                int _target_colony_defense_value = 0;
                int _last_target_fire_power = 999995;
                int _new_target_fire_power = 999994;

                int _next_target_fire_power = 999993;
                int _lowest_defense_value = 999992;
                string _text_header = "";
                string _all_attack_location_text = "";
                //int _targetDistance = 99;

                if (_player_is_human)
                {
                    //Debugger.Break();
                }

                if (_civM_1.Target_CivList != null && _civM_1.Target_CivList.Count > 0)
                {
                    if (_civM_1.Target_CivList[0].Key == _civ1.LongName)
                    {
                        goto Skipped_Target_Colony;
                    }


                    foreach (var _item in _civM_1.Target_CivList)
                    {
                        MapLocation _loc_1 = _civM_1.HomeSystem.Location;

                        var _civ2Colonies = _civM_2.Colonies.ToList();
                        //_lowest_targetDistance = 99;

                        foreach (var _colony in _civ2Colonies)
                        {
                            MapLocation _loc_2 = _colony.Location;
                            int _distance = (int)Math.Sqrt((int)Math.Pow(_loc_1.X - _loc_2.X, 2)
                                + (int)Math.Pow(_loc_1.Y - _loc_2.Y, 2));
                            int _defense_value = Colony.DefenseValue(_colony); // minimum defense value
                            //_defense_value = UnitAI.
                            if (_loc_2 == _loc_1)
                            {
                                _distance += 50;
                            }
                            if (!_target_ColoniesLocations.ContainsKey(_colony.Location))
                            {
                                if (GameContext.Current.CivilizationManagers[_item.CivID].SeatOfGovernment != null)  // subjageted
                                {
                                    _target_ColoniesLocations.Add(_colony.Location, new ColonyTargetValues(_distance, _defense_value));
                                }
                                //_target_ColoniesLocations.Add(_civM_1.HomeColony.Location, new ColonyTargetValues(99, 999999));
                            }



                        }
                        //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }
                    }

                    if (_player_is_human)
                    {
                        //Debugger.Break();
                    }

                    //int _minValue = 98;

                    //MapLocation _new_assault_location = new MapLocation();
                    // \r\n
                    _text_header = "Step_7726:; Assault_Location"
                                    //+ "_civM_1.Assault_Location"Do_13_Diplomacy > 

                                    + " for >>> "
                                    + _civ1 + " at " + GameEngine.LocationString(_civM_1.HomeSystem.Location.ToString())
                                    + " vs " + _civ2
                                    + "= " + _civM_2.Colonies.Count + " colonies"
                                    ;

                    foreach (var item in _target_ColoniesLocations)
                    {

                        //if (item.Value < _minValue)
                        //{
                        //    _minValue = item.Value;

                        //string _distance_text = "";


                        if (_regard < 420)
                        {
                            //_targetDistance = MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location);

                            //if (_targetDistance < MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location))
                            //{
                            _targetDistance = MapLocation.GetDistance(_civM_1.HomeSystem.Location, item.Key);
                            //_civM_1.Assault_TargetCiv = GameContext.Current.CivilizationManagers[item.Key].Civilization;

                            if (_targetDistance > 0 && _targetDistance < _lowest_targetDistance)
                            {
                                //_text = "Step_7720:; Do_13_Diplomacy > "
                                //        /*+ "_targetDistance= "*/
                                //        + _civM_1.Civilization.Key
                                //        + " > " + item.Key
                                //        + " > _targetDistance= " + _targetDistance
                                //        + ", _lowest_targetDistance= " + _lowest_targetDistance
                                //            ;
                                //if (_writeDirectly)
                                //Console.WriteLine(_text);
                                //_all_attack_location_text += _newline + _text;

                                if (_player_is_human)
                                {
                                    //Debugger.Break();
                                }

                                _lowest_targetDistance = _targetDistance;
                                //GameEngine.LocationFirePower(item.Key, out _target_colony_defense_value);

                                List<Colony> _colony_there = GameContext.Current.Universe.Find<Colony>()//(_civ).ToList()
                                                            .Where(a => a.Location.ToString() == item.Key.ToString())
                                                            .ToList()
                                                            ;

                                Colony _colony_local = null;

                                //Debugger.Break();

                                //try
                                //{
                                if (_colony_there.Count > 0)
                                {
                                    _colony_local = _colony_there[0];
                                }

                                //}
                                //catch
                                //{
                                //    Debugger.Break(); // no _colony is this sector > just a station ?
                                //}

                                _text = "";
                                //string _text_header ="";

                                if (_colony_local != null)
                                {
                                    _target_colony_defense_value += Colony.DefenseValue(_colony_local);

                                    _text += Environment.NewLine
                                    + "Step_7727:; possible  > "
                                    + " Colony= " + _colony_local.ObjectID + " at " + _colony_local.LocationStringColony
                                    + " "
                                    + _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/

                                    + "   ; Defense= " + GameEngine.Do_x_Digit_String(5, _target_colony_defense_value.ToString())

                                    //+ "   ; _regard= " + _regard
                                    + "   ; Attack= " + _civM_1.Assault_Attack_Value
                                    + "   ; Distance= " + GameEngine.Do_x_Digit_String(2, _targetDistance.ToString())
                                    + "   ; _next_target_fire_power= " + GameEngine.Do_x_Digit_String(5, _next_target_fire_power.ToString())
                                    + "   ; AssVal_Defense+Dist= " + GameEngine.Do_x_Digit_String(5, _civM_1.Assault_Value_Defense_and_Distance.ToString())
                                        //+ "XXXXX >"
                                        //+ " Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())
                                        //+ " for "
                                        //+ _civ1 + " at " + LocationString(_civM_1.HomeSystem.Location.ToString()) + "   ; vs ; "
                                        //+ _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/
                                        ////+ "   ; _regard= " + _regard
                                        //+ "; Colony= " + item.Key
                                        //+ "; _target_colony_defense_value= " + GameEngine.Do_x_Digit_String( 5, (_target_colony_defense_value.ToString())
                                        ////+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                                        ////+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                                        ;
                                }

                                _next_target_fire_power = _target_colony_defense_value
                                    + ((_targetDistance * _targetDistance) * 100);


                                //if (_writeDirectly)
                                //Console.WriteLine(_text);
                                _all_attack_location_text += string.Concat(_all_attack_location_text, _text);
                                //_distance_text += /*_newline +*/ _text;
                                //Console.WriteLine(_all_attack_location_text);  see below


                            }



                            //Console.WriteLine(_all_attack_location_text + " > from Step_7722");

                            //if (_civ1.IsHuman)
                            //{
                            //    Debugger.Break();
                            //}
                            ////}


                            //Console.WriteLine(_all_attack_location_text + " > from Step_7723");

                            //if (_civ1.IsHuman)
                            //{
                            //    Debugger.Break();
                            //}


                            _next_target_fire_power = _target_colony_defense_value + ((_targetDistance + 1) * 100); // gives a 100 basic value

                            if (_next_target_fire_power < _last_target_fire_power)
                            {
                                _new_assault_location = item.Key;
                                _lowest_defense_value = _next_target_fire_power;

                                _last_target_fire_power = _lowest_defense_value;
                            }
                        }

                        if (_player_is_human)
                        {
                            //Debugger.Break();
                        }
                        //Skipped_Target_Colony:;
                    }


                    if (_all_attack_location_text != "")
                    {
                        Console.WriteLine(_text_header + _all_attack_location_text + " > from Step_7725"); // see below
                    }

                    if (_player_is_human)
                    {
                        //Debugger.Break();
                    }

                }
            Skipped_Target_Colony:;

                //if (_all_attack_location_text != "")
                //{
                //    Console.WriteLine(_text_header + _all_attack_location_text + " > from Step_7725"); // see below
                //}

                if (/*_civM_1.Assault_Location != _new_assault_location && */_new_target_fire_power < _civM_1.Assault_Value_Defense_and_Distance)
                {


                    _text = "Step_7717:; Do_13_Diplomacy > "
                            + "_civM_1.Assault_Location"

                            + " for >>> "
                            + _civ1 + " at " + GameEngine.LocationString(_civM_1.HomeSystem.Location.ToString())

                            + " possible  > "
                            + " Colony= " + GameEngine.LocationString(_new_assault_location.ToString())
                            + " "
                            + _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/

                            + "   ; Defense= " + GameEngine.Do_x_Digit_String(5, _civM_1.Assault_DefenseValue.ToString())

                            //+ "   ; _regard= " + _regard
                            + "   ; Attack= " + _civM_1.Assault_Attack_Value //GameEngine.Do_x_Digit_String(5, _civM_1.Assault_AttackValue.ToString())
                                                                             //+ "  ; Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())

                                //+ "_civM_1.Assault_Location possible  >"
                                ////+ " Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())
                                //+ " for "
                                //+ _civ1 + " at " + LocationString(_civM_1.HomeSystem.Location.ToString()) + "   ; vs ; "
                                //+ _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/
                                //+ " for Colony= " + _new_assault_location
                                ////+ "   ; _regard= " + _regard

                                //+ "; _target_colony_defense_value= " + _target_colony_defense_value
                                //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                                //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                                ;
                    //if (_writeDirectly) 
                    //Console.WriteLine(_text);
                    //_all_attack_location_text += _newline + _text;


                    _civM_1.Assault_Location = _new_assault_location;
                    _civM_1.Assault_Value_Defense_and_Distance = _new_target_fire_power;

                    _last_target_fire_power = _new_target_fire_power;

                    if (_player_is_human)
                    {
                        //Debugger.Break();
                    }

                }


                //Console.WriteLine(_all_attack_location_text + "        > from Step_7725");


                //if (_writeDirectly)
                //{
                //    Console.WriteLine(_text + "            > from Step_7727");
                //}

                if (_player_is_human)
                {
                    //Debugger.Break();
                }

                //if (_writeDirectly) Console.WriteLine("Step_7727:; _diplomacyBasicsSummary_Text="
                //    + _diplomacyBasicsSummary_Text + _newline
                //    + "End of _diplomacyBasicsSummary_Text" + _newline

                //    );
                //_diplomacyBasicsSummary_Text = "";

                string _atWarText = "";

                if (_foreignPowerStatus == ForeignPowerStatus.AtWar
                        && _civM_1.Target_CivList != null
                        && _civM_1.Target_CivList.Count > 0)
                {

                    //_target_ColoniesLocations = new Dictionary<Civilization, int>(); // find out the nearest one
                    //_target_ColoniesLocations.Add(_civ1, 99);  // avoid an empty Dictionary


                    //foreach (var _item in _civM_1.Target_CivList)
                    //{
                    //    MapLocation _loc_1 = _civM_1.HomeSystem.Location;
                    //    MapLocation _loc_2 = GameContext.Current.CivilizationManagers[_item.CivID].HomeSystem.Location;
                    //    int _distance = (int)Math.Sqrt((int)Math.Pow(_loc_1.X - _loc_2.X, 2)
                    //        + (int)Math.Pow(_loc_1.Y - _loc_2.Y, 2));
                    //    if (_loc_2 == _loc_1)
                    //    {
                    //        _distance += 50;
                    //    }
                    //    if (!_target_ColoniesLocations.ContainsKey(_item))
                    //    {
                    //        if (GameContext.Current.CivilizationManagers[_item.CivID].SeatOfGovernment != null)  // subjageted
                    //        {
                    //            _target_ColoniesLocations.Add(_item, _distance);
                    //        }

                    //    }
                    //    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }
                    //}

                    //Debugger.Break()
                    //if (_civ1.IsHuman && _foreignPowerStatus != ForeignPowerStatus.NoContact) { Debugger.Break(); }

                    //var _nearest_target = _target_ColoniesLocations.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

                    //_civM_1.Assault_TargetCiv = _nearest_target;
                    //_text = "Step_7737:; Do_13_Diplomacy > "
                    //    + "AtWar > nearest Target: "
                    //    + _civ1 + " at " + _civM_1.HomeSystem.Location + "; vs ; "
                    //    + _civ2 + " at " + _civM_2.HomeSystem.Location
                    //    //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location
                    //    + ", Distance= " + MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location)
                    //    //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                    //    ;
                    //if (_writeDirectly) Console.WriteLine(_text);



                    if (_player_is_human)
                    {
                        //Debugger.Break();
                    }

                    //int _minValue = 98;
                    //foreach (var item in _target_ColoniesLocations)
                    //{
                    //    if (item.Value < _minValue)
                    //    {
                    //        _minValue = item.Value;
                    //        _civM_1.Assault_TargetCiv = item.Key;

                    //        _text = "Step_7736:; Do_13_Diplomacy > "
                    //                    + "AtWar >"
                    //                    + " Distance= " + GameEngine.Do_x_Digit_String(MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location).ToString())
                    //                    + " for "
                    //                    + _civ1 + " at " + _civM_1.HomeSystem.Location + "    ; vs ; "
                    //                    + _civ2 + " at " + _civM_2.HomeSystem.Location

                    //                    + "; Assault_TargetCiv= " + _civM_1.Assault_TargetCiv
                    //                    //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                    //                    //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                    //                    ;
                    //        if (_writeDirectly) Console.WriteLine(_text);
                    //        _diplomacyBasicsSummary_Text += _newline + _text;

                    //        //if (_civ1.IsHuman) { Debugger.Break(); }

                    //    }
                    //}


                    _text = "Step_7737:; Do_13_Diplomacy > "

                            + _civ1 + " at " + _civM_1.HomeSystem.Location /*+ "; vs ; "*/
                            //+ " nearest Target: "
                            + "; AtWar > Distance= * " + GameEngine.Do_x_Digit_String(2, MapLocation.GetDistance(_civM_1.HomeSystem.Location, _civM_2.HomeSystem.Location).ToString())
                            + " * for " + _civ2 + " at " + _civM_2.HomeSystem.Location
                            + " <<<<<<<<<<<<<<<<<<"
                            ;
                    //if (_writeDirectly) Console.WriteLine(_text);
                    _diplomacyBasicsSummary_Text += Environment.NewLine + _text;
                    _atWarText = _text;

                    if (_player_is_human)
                    {
                        //Debugger.Break(); 
                    }


                    //Console.WriteLine(_newline + "Step_7734:; Begin of _diplomacyBasicsSummary_Text" + /*_newline + */_diplomacyBasicsSummary_Text + _newline + "End of _diplomacyBasicsSummary_Text" + _newline);

                    //if (_civ1.IsHuman) { Debugger.Break(); }

                    if (_player_is_human
                        //&& _foreignPowerStatus != ForeignPowerStatus.NoContact
                        && _foreignPowerStatus != ForeignPowerStatus.AtWar)
                    {
                        if (_atWarText == "") _atWarText = "Step_7738:; Do_13_Diplomacy > with nobody for " + _civ1;
                        if (_writeDirectly) Console.WriteLine(_atWarText);
                        //Debugger.Break();
                    }
                }


                //Console.WriteLine(_all_attack_location_text + "        > from Step_7725 = _all_attack_location_text");



                _text = "Step_7718:; Do_13_Diplomacy > "
                            + "_civM_1.Assault_Location"
                            //+ " Distance= " + GameEngine.Do_x_Digit_String(_targetDistance.ToString())
                            + " for >>> "
                            + _civ1 + " at " + GameEngine.LocationString(_civM_1.HomeSystem.Location.ToString())

                            + " possible  > "
                            + " Colony= " + GameEngine.LocationString(_new_assault_location.ToString())
                            + " "
                            + _civM_2.Civilization /*+ " at " + LocationString(item.Key.ToString())*/

                            + "   ; Defense= " + GameEngine.Do_x_Digit_String(5, _civM_1.Assault_DefenseValue.ToString())

                            ////+ "   ; _regard= " + _regard
                            + "   ; Attack= " + _civM_1.Assault_Attack_Value //GameEngine.Do_x_Digit_String(5, _civM_1.Assault_AttackValue.ToString())
                            + "   ; Distance= " + GameEngine.Do_x_Digit_String(2, MapLocation.GetDistance(_civM_1.HomeSystem.Location, _new_assault_location).ToString())

                            //+ "; _target_colony_defense_value= " + _target_colony_defense_value
                            //+ _civ2 + " at " + GameContext.Current.CivilizationManagers[_civM_1.Assault_TargetCiv].HomeSystem.Location

                            //+ ", Distance=" + MapLocation.GetDistance(_civM_1.HomeSystem.Location, GameContext.Current.CivilizationManagers[_civ2.CivID].HomeSystem.Location)

                            ;
                //if (_writeDirectly) 
                //Console.WriteLine(_text);


                if (_player_is_human)
                {
                    //Debugger.Break();
                }


                //var _nearest_target = _target_ColoniesLocations.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;
                //// doubled))
                //if (l.Value < r.Value)
                //{
                //    _civM_1.Assault_TargetCiv = _target_ColoniesLocations.Aggregate((l, r) => l).Key;
                //}
                //else
                //{
                //    _civM_1.Assault_TargetCiv = _target_ColoniesLocations.Aggregate((l, r) => r).Key;
                //}





                //Borg                    
                //if (_itIsBorg == true)
                ////{
                //if (_civ1.CivID == 6 || _civ1.Key == "BORG")
                //{
                //    //var aForeignPower = _diplomat1.GetForeignPower(_civ2);
                //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                //    continue;
                //}
                //if (_civ2.CivID == 6 || _civ2.Key == "BORG")
                //{
                //    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                //    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                //    continue;
                //}
                //_text = "Step_7720:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Borg involved";
                ////if (_writeDirectly) Console.WriteLine(_text);
                ////continue;
                ////}

                //Console.WriteLine(_newline + "Step_7734:; Begin of _diplomacyBasicsSummary_Text" + _newline + _diplomacyBasicsSummary_Text + _newline + "End of _diplomacyBasicsSummary_Text" + _newline);




                //DoBorgDiploApply(_civ1, _civ2); // not worth
                ////Borg                    
                if (_civ1.CivID == 6 || _civ1.Key == "BORG")
                {
                    //var aForeignPower = _diplomat1.GetForeignPower(_civ2);
                    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                    continue;
                }
                if (_civ2.CivID == 6 || _civ2.Key == "BORG")
                {
                    DiplomacyHelper.ApplyTrustChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);
                    DiplomacyHelper.ApplyRegardChange(_diplomatForeignPower_Civ2.Counterparty, _diplomatForeignPower_Civ2.Owner, -1000);

                    continue;
                }
                //_text = "Step_7720:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > Borg involved";
                ////if (_writeDirectly) Console.WriteLine(_text);
                //continue;
                //}


                Diplomat diplomat2 = Diplomat.Get(_civ2);
                string _Contact = "";
                if (_diplomat1.GetForeignPower(_civ2).DiplomacyData.Status == ForeignPowerStatus.NoContact ||
                    diplomat2.GetForeignPower(_civ1).DiplomacyData.Status == ForeignPowerStatus.NoContact)
                {
                    _Contact = " > No Contact !";
                    //_text = "Step_7710:; Do_13_Diplomacy > " + _civ1 + "; vs; " + _civ2 + "; > NoContact";
                    //if (_writeDirectly) Console.WriteLine(_text);
                    //GameLog.Core.DiplomacyDetails.DebugFormat("DiplomacyData.Status = NoContact for {0} vs {1}", _civ1, _civ2);
                    continue;
                }

                //if (_civ1.Key == "FEDERATION" || _civ2.Key == "FEDERATION")
                //{
                //    _checkRace = true;
                //    _text = "Step_7702:; Do_13_Diplomacy > * " + _civ1.Key + " * vs * " + _civ2.Key
                //        + " > " + _foreignPowerStatus
                //        + "" + _Contact
                //        ;
                //    //if (_writeDirectly)
                //    Console.WriteLine(_text);

                //    Debugger.Break();
                //}
                //#endregion DiplomacyBasics





                //DiplomacyDoStatus(_civ1, _civ2);  // like War


                Diplomacy_2_PendingActions(_civ1, _civ2);


                //var _diplomacyData = GameContext.Current.DiplomacyData[_civ1, _civ2];

                //string _diplomat1_Location_String = "( Empire )";
                //if (!_diplomat1.Owner.IsEmpire && _diplomat1.SeatOfGovernment != null)
                //    _diplomat1_Location_String = _diplomat1.SeatOfGovernment.Location.ToString();

                //// SitRep for all
                //if (_foreignPowerStatus != ForeignPowerStatus.OwnerIsSubjugated)
                //{
                //    _text = "Relation > "
                //        + "Regard: " + GameEngine.Do_x_Digit_String(4, _diplomacyData.Regard.CurrentValue.ToString())
                //        + ", Trust: " + GameEngine.Do_x_Digit_String(4, _diplomacyData.Trust.CurrentValue.ToString())

                //        + " > " + _foreignPowerStatus

                //        + " vs " + _diplomat1.Owner
                //        + " " + _diplomat1_Location_String
                //        ;
                //    // too much info
                //    //Console.WriteLine("Step_7429:; " + _text + "; Turn " + GameContext.Current.TurnNumber + ";SR for " + _civ2.Name);

                //    GameContext.Current.CivilizationManagers[_civ2].SitRepEntries.Add(
                //        new ReportEntry_ShowDiplo(_civ2, _text, "", "", SitRepPriority.GreenDark2));

                //}

                //string _testCiv = "FEDERATION";
                string _testCiv = "BORG";
                if (_civ1.Key == _testCiv || _civ2.Key == _testCiv)
                {
                    //_checkRace = true;
                    _text = "Step_7702:; Do_13_Diplomacy > * " + _civ1.Key + " * vs * " + _civ2.Key
                        + " > " + _foreignPowerStatus
                        + "" + _Contact
                        + Environment.NewLine
                        ;
                    //if (_writeDirectly)
                    Console.WriteLine(_text);
                    _diplomacyBasicsSummary_Text += _text;

                    //Debugger.Break();
                }

                //Console.WriteLine(_text_header + _all_attack_location_text + " > from Step_7703 = _all_attack_location_text");

                //Debugger.Break();
            }


        }

        public static void Diplomacy_2_PendingActions(Civilization _civ1, Civilization _civ2)
        {
            Diplomat diplomat1 = Diplomat.Get(_civ1);

            ForeignPower _diplomatCiv2 = diplomat1.GetForeignPower(_civ2);
            ForeignPowerStatus _foreignPowerStatus = diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

            string _text;

            //GameLog.Core.DiplomacyDetails.DebugFormat("---------------------------------------");
            //GameLog.Core.DiplomacyDetails.DebugFormat("_foreignPowerStatus = {2} for {0} vs {1}", _civ1, _civ2, _foreignPowerStatus.ToString());

            try
            {


                //_text = "Step_7720:; Do_13_Diplomacy > * " + _civ1.Key 
                //        + " * vs * " + _civ2.Key
                //        + ": PendingAction > Status= >>> " + _civ2_foreign_power.PendingAction.ToString()
                //        ;
                ////if (_writeDirectly)
                //Console.WriteLine(_text);


                //if (_checkRace) Debugger.Break();



                switch (_diplomatCiv2.PendingAction)
                {
                    case PendingDiplomacyAction.None:
                        //_text = "Step_7721:; Do_13_Diplomacy > * " + _civ1.Key + " * vs * " + _civ2.Key
                        //        + ": PendingAction > Status= >>> " + _civ2_foreign_power.PendingAction.ToString()
                        //        ;
                        //Console.WriteLine(_text);
                        break;


                    case PendingDiplomacyAction.AcceptProposal:
                        {
                            _text = "Step_7722:; Do_13_Diplomacy > * " + _civ1.Key + " * vs * " + _civ2.Key
                                + ", Accept Status=" + _diplomatCiv2.PendingAction.ToString()
                                ;
                            //if (_writeDirectly)
                            Console.WriteLine(_text);
                            //Debugger.Break();
                            //GameLog.Core.DiplomacyDetails.DebugFormat(_text);

                            if (_diplomatCiv2.ProposalReceived != null)
                            {
                                _ = AcceptProposalVisitor.Visit(_diplomatCiv2.ProposalReceived);
                            }

                            _diplomatCiv2.LastProposalReceived = _diplomatCiv2.ProposalReceived;
                            _diplomatCiv2.ProposalReceived = null;
                            break;
                        }

                    case PendingDiplomacyAction.RejectProposal:
                        {
                            _text = "Step_7724:; Do_13_Diplomacy > * "
                                    + _civ1.Key + " * vs * " + _civ2.Key
                                    + ", Reject Status=" + _diplomatCiv2.PendingAction.ToString()
                                    ;
                            //if (_writeDirectly)
                            Console.WriteLine(_text);

                            Debugger.Break();
                            //GameLog.Core.DiplomacyDetails.DebugFormat(_text);

                            if (_diplomatCiv2.ProposalReceived != null)
                            {
                                RejectProposalVisitor.Visit(_diplomatCiv2.ProposalReceived);
                            }

                            _diplomatCiv2.LastProposalReceived = _diplomatCiv2.ProposalReceived;
                            _diplomatCiv2.ProposalReceived = null;
                            break;
                        }
                    default:  // case None
                        break;
                }




                //GameLog.Core.DiplomacyDetails.DebugFormat("Next: _diplomatForeignPower_Civ2.PendingAction = NONE for {0} vs {1}
                //, status {2}, pending {3}", _diplomatForeignPower_Civ2.Owner, _diplomatForeignPower_Civ2.Counterparty
                //, _foreignPowerStatus.ToString(), _diplomatForeignPower_Civ2.PendingAction.ToString());

                if (_diplomatCiv2.PendingAction != PendingDiplomacyAction.None)
                {
                    _text = "Step_4335:; "
                        + "Next: _diplomatForeignPower_Civ2.PendingAction = NONE for " + _diplomatCiv2.Owner
                        + " vs " + _diplomatCiv2.Counterparty
                        + ", status=" + _foreignPowerStatus.ToString()
                        + ", pending= " + _diplomatCiv2.PendingAction.ToString()

                        ;
                    Console.WriteLine(_text);
                }

                _diplomatCiv2.PendingAction = PendingDiplomacyAction.None;

                // Ships gets new owner on joining empire - _colonies are done in AccpetPropsalVisitor
                if (_civ1.IsEmpire && !_civ2.IsEmpire && _civ1.Key != "Borg")
                {
                    Diplomat currentDiplomat = Diplomat.Get(_civ1);

                    // for ForeignPowerStatus.CounterpartyIsMember
                    if (currentDiplomat.GetForeignPower(_civ2).DiplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember)
                    {
                        //_text = "Searching for Crash: _objectsCiv2";
                        //if (_writeDirectly) Console.WriteLine(_text);
                        List<UniverseObject> _objectsCiv2 = GameContext.Current.Universe.Objects.Where(s => s.Owner == _civ2)
                                .Where(s => s.ObjectType == UniverseObjectType.Ship).ToList();
                        foreach (UniverseObject minorsObject in _objectsCiv2)
                        {
                            if (minorsObject.Owner == _civ2)
                            {
                                CivilizationManager targetMinor = GameContext.Current.CivilizationManagers[_civ2];
                                Colony minorCivHome = targetMinor.HomeColony;
                                int gainedResearchPoints = minorCivHome.Research_Net;
                                Ship ship = (Ship)minorsObject;
                                ship.Owner = _civ1;
                                Fleet newfleet = ship.CreateFleet();
                                newfleet.Owner = _civ1;
                                newfleet.SetOrder(FleetOrders.IdleOrder.Create());
                                if (newfleet.Order == null)
                                {
                                    newfleet.SetOrder(FleetOrders.IdleOrder.Create());
                                }
                                ship.Scrap = false;
                                GameContext.Current.CivilizationManagers[_civ1].Research.UpdateResearch(gainedResearchPoints);

                                _text = "Civ2= " + _civ2
                                    + " got MEMBER "
                                    + " and we won " + gainedResearchPoints
                                    + " by getting " + minorsObject.ObjectID
                                    + " " + minorsObject.ObjectID

                                    ;
                                Console.WriteLine("Step_5434:; " + _text);

                                //GameLog.Core.Ships.DebugFormat("Ship Joined:{0} {1}, Owner {2}, OwnerID {3}, Fleet.OwnerID {4}, Order {5} _fleet name {6} gainedResearchPoints {7}",
                                //        ship.ObjectID, ship.Name, ship.Owner, ship.OwnerID, newfleet.OwnerID, newfleet.Order, newfleet.Name, gainedResearchPoints);
                            }
                        }
                    }

                }  // foreach _civ2
            }
            catch (Exception e)
            {
                Console.WriteLine("Step_9223:; " + e);
                Debugger.Break();
            }
        }

        public static void Diplomacy_4_Statement_Received(Statement _statement_received)
        {
            Civilization _civ1 = _statement_received.Sender;
            Civilization _civ2 = _statement_received.Recipient;

            Diplomat _diplomat1 = Diplomat.Get(_civ1);

            ForeignPower _diplomatCiv2 = _diplomat1.GetForeignPower(_civ2);
            //ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;
            string _text = "Step_4534:; 4_Statement_Received from " + _civ1
                            + " to " + _civ2

                            ;
            Console.WriteLine("Step_5434:; " + _text);

            switch (_statement_received.StatementType)
            {
                case StatementType.WarPact:
                case StatementType.CommendWar:
                case StatementType.DenounceWar:
                case StatementType.WarDeclaration:
                    break;
                case StatementType.StealCredits:
                    IntelHelper.SabotageStealCreditsExecute(_civ2, _civ1,
                        _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    break;
                case StatementType.StealResearch:
                    IntelHelper.SabotageStealResearchExecute(_civ2, _civ1,
                        _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    break;
                case StatementType.SabotageFood:
                    //if (_civ2.CivID > _civ1.CivID)
                    //{
                    IntelHelper.SabotageFoodExecute(_civ2, _civ1,
                        _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    //}

                    break;
                case StatementType.SabotageIndustry:
                    //if (_civ2.CivID > _civ1.CivID)
                    //{
                    IntelHelper.SabotageIndustryExecute(_civ2, _civ1,
                        _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    //}

                    break;
                case StatementType.SabotageEnergy:
                    //if (_civ2.CivID > _civ1.CivID)
                    //{
                    IntelHelper.SabotageEnergyExecute(_civ2, _civ1,
                        _diplomatCiv2.StatementReceived.Parameter.ToString(), 99999);
                    //}

                    break;

                // T01 = _civ1 0 vs _civ1 1 (up to 5 without 6=Borg)
                case StatementType.T01: // read statement type off of _diplomatForeignPower_Civ2 and send it to accept - reject dictionary
                case StatementType.T02:
                case StatementType.T03:
                case StatementType.T04:
                case StatementType.T05:
                case StatementType.T10:
                case StatementType.T12:
                case StatementType.T13:
                case StatementType.T14:
                case StatementType.T15:
                case StatementType.T20:
                case StatementType.T21:
                case StatementType.T23:
                case StatementType.T24:
                case StatementType.T25:
                case StatementType.T30:
                case StatementType.T31:
                case StatementType.T32:
                case StatementType.T34:
                case StatementType.T35:
                case StatementType.T40:
                case StatementType.T41:
                case StatementType.T42:
                case StatementType.T43:
                case StatementType.T45:
                case StatementType.T50:
                case StatementType.T51:
                case StatementType.T52:
                case StatementType.T53:
                case StatementType.T54:
                case StatementType.F01:
                case StatementType.F02:
                case StatementType.F03:
                case StatementType.F04:
                case StatementType.F05:
                case StatementType.F10:
                case StatementType.F12:
                case StatementType.F13:
                case StatementType.F14:
                case StatementType.F15:
                case StatementType.F20:
                case StatementType.F21:
                case StatementType.F23:
                case StatementType.F24:
                case StatementType.F25:
                case StatementType.F30:
                case StatementType.F31:
                case StatementType.F32:
                case StatementType.F34:
                case StatementType.F35:
                case StatementType.F40:
                case StatementType.F41:
                case StatementType.F42:
                case StatementType.F43:
                case StatementType.F45:
                case StatementType.F50:
                case StatementType.F51:
                case StatementType.F52:
                case StatementType.F53:
                case StatementType.F54:
                    {
                        GameLog.Core.DiplomacyDetails.DebugFormat("Statement sent for Dictionary Entery {0} _diplomatForeignPower_Civ2 Counterparty {1}, Owner {2}",
                            Enum.GetName(typeof(StatementType), _diplomatCiv2.StatementReceived.StatementType),
                            _diplomatCiv2.Counterparty.Key,
                            _diplomatCiv2.Owner.Key);

                        DiplomacyHelper.SpecificCivAcceptingRejecting(_diplomatCiv2.StatementReceived.StatementType); // act on statement to accept reject
                        break;
                    }

                default:
                    break;
            }
            //else
            //{
            //else

            ////  Second.1 = StatementReceived
            if (_diplomatCiv2.StatementReceived == null && _diplomatCiv2.LastStatementReceived != null)
            {
                switch (_diplomatCiv2.LastStatementReceived.StatementType)
                {
                    case StatementType.StealCredits:
                        IntelHelper.SabotageStealCreditsExecute(_civ2, _civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.StealResearch:
                        IntelHelper.SabotageStealResearchExecute(_civ2, _civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.SabotageFood:
                        IntelHelper.SabotageFoodExecute(_civ2, _civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.SabotageIndustry:
                        IntelHelper.SabotageIndustryExecute(_civ2, _civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    case StatementType.SabotageEnergy:
                        IntelHelper.SabotageEnergyExecute(_civ2, _civ1, _diplomatCiv2.LastStatementReceived.Parameter.ToString(), 99999);
                        _diplomatCiv2.LastStatementReceived = null;
                        break;
                    //    GameLog.Core.DiplomacyDetails.DebugFormat("LastStatementReceived Statement Type = {0} _diplomatForeignPower_Civ2 counterparyt {1}, owner {2}",
                    //        Enum.GetName(typeof(StatementType), _diplomatForeignPower_Civ2.LastStatementReceived.StatementType),
                    //        _diplomatForeignPower_Civ2.Counterparty.Key,
                    //        _diplomatForeignPower_Civ2.Owner.Key);
                    //    break;
                    case StatementType.CommendWar:
                    case StatementType.DenounceWar:
                    case StatementType.WarDeclaration:
                        break;
                    default:
                        break;
                }
                //}
            }
        }

        public static void Diplomacy_7_Response_Sent(IResponse _response)
        {
            Civilization civ1 = _response.Sender;
            Civilization civ2 = _response.Recipient;

            Diplomat diplomat1 = Diplomat.Get(civ1);

            ForeignPower _diplomatCiv2 = diplomat1.GetForeignPower(civ2);
            string _text = "";
            string _sender_civ = "";
            string _recipient_civ = "";
            bool _writeDirectly = true;

            IResponse responseSent = _diplomatCiv2.ResponseSent;
            if (responseSent != null)
            {
                _diplomatCiv2.CounterpartyForeignPower.ResponseReceived = responseSent; // cross over response sent to response received
                _text = "Step_7721:; "
                    + _diplomatCiv2.Owner.Key
                    + " sent Response " + _diplomatCiv2.ResponseSent.Proposal.ToString()
                    + " to " + _diplomatCiv2.Counterparty.Key
                    ;
                if (_writeDirectly) Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("{0} sent Response {1} to {2}"
                //    , _diplomatForeignPower_Civ2.Owner.Key, _diplomatForeignPower_Civ2.ResponseSent.Proposal.ToString(), _diplomatForeignPower_Civ2.Counterparty.Key);
                _diplomatCiv2.LastResponseSent = responseSent;
                _text = "Step_7722:;"
                        // + _diplomatForeignPower_Civ2.Owner.Key
                        + " > Response Sent stored in LastResponseSent " + _diplomatCiv2.ResponseSent.ToString()
                        ;
                //if (_writeDirectly) 
                //Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("Response Sent stored in LastResponseSent, {0}", _diplomatForeignPower_Civ2.ResponseSent.ToString());
                _diplomatCiv2.ResponseSent = null;

                if (responseSent.ResponseType != ResponseType.NoResponse &&
                    !(responseSent.ResponseType == ResponseType.Accept && responseSent.Proposal.IsGift()))
                {
                    _sender_civ = responseSent.Sender.ToString();
                    _recipient_civ = responseSent.Recipient.ToString();
                    //if (_civ1.IsEmpire)
                    //{
                    GameContext.Current.CivilizationManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, responseSent));
                    //}

                    //if (_civ2.IsEmpire)
                    //{
                    GameContext.Current.CivilizationManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, responseSent));
                    //}
                }
                else if (responseSent.ResponseType != ResponseType.NoResponse && responseSent.ResponseType == ResponseType.Reject)
                {
                    _sender_civ = responseSent.Sender.ToString();
                    _recipient_civ = responseSent.Recipient.ToString();
                    if (civ1.IsEmpire)
                    {
                        GameContext.Current.CivilizationManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, responseSent));
                    }

                    if (civ2.IsEmpire)
                    {
                        GameContext.Current.CivilizationManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, responseSent));
                    }
                }
            }
            else
            {
                _diplomatCiv2.CounterpartyForeignPower.ResponseReceived = null;
            }
        }

        public static void Diplomacy_6_Statement_Sent(Statement statement)
        {
            string _text;
            bool _writeDirectly = true;

            Civilization civ1 = statement.Sender;
            Civilization civ2 = statement.Recipient;

            Diplomat _diplomatCiv1 = Diplomat.Get(civ1);
            CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[civ1];

            ForeignPower _diplomatCiv2 = _diplomatCiv1.GetForeignPower(civ2);
            CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[civ2];

            ForeignPowerStatus _foreignPowerStatus = _diplomatCiv1.GetForeignPower(civ2).DiplomacyData.Status;

            Statement statementSent = _diplomatCiv2.StatementSent;
            if (statementSent != null)
            {
                // StatementSent becomes counterparty StatementReceived
                _diplomatCiv2.CounterpartyForeignPower.StatementReceived = statementSent;
                _text = "Step_8236:; StatementReceived= "
                        + "; _diplomatForeignPower_Civ2.Owner= " + _diplomatCiv2.CounterpartyForeignPower.Owner.Key
                        + "; got * " + Enum.GetName(typeof(StatementType), statementSent.StatementType)
                        + " * ; from= " + statementSent.Sender.Key
                        ;
                if (_writeDirectly)
                    Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("_diplomatForeignPower_Civ2.Owner {0} got StatementReceived {1} from {2}"
                //    , _diplomatForeignPower_Civ2.CounterpartyForeignPower.Owner.Key
                //    , Enum.GetName(typeof(StatementType), statementSent.StatementType)
                //    , statementSent.Sender.Key);
                _diplomatCiv2.LastStatementSent = statementSent;
                _diplomatCiv2.StatementSent = null;

                //GameLog.Core.DiplomacyDetails.DebugFormat("_diplomatForeignPower_Civ2.Owner = {0}", _diplomatForeignPower_Civ2.Owner.Key);
                //GameLog.Core.DiplomacyDetails.DebugFormat("CounterpartyForeignPower.Owner = {0}", _diplomatForeignPower_Civ2.CounterpartyForeignPower.Owner.Key);

                bool _do_DeclareWar = false;

                if (statementSent.StatementType == StatementType.WarDeclaration)
                {
                    _do_DeclareWar = true;

                }

                if (_civM_1.Assault_Location != null && _foreignPowerStatus != ForeignPowerStatus.AtWar)
                {
                    _do_DeclareWar = true;
                }

                if (_do_DeclareWar == true && !civ1.IsHuman)
                {
                    _diplomatCiv2.DeclareWar();  // GameEngine
                }
                _diplomatCiv2.CounterpartyForeignPower.StatementReceived = null;
            }
            else
            {
                _diplomatCiv2.CounterpartyForeignPower.StatementReceived = null; // ??
            }
        }

        public static void Diplomacy_5_Proposal_Sent(IProposal _proposalSent)
        {
            Civilization _civ1 = _proposalSent.Sender;
            Civilization _civ2 = _proposalSent.Recipient;

            Diplomat _diplomat_civ1 = Diplomat.Get(_civ1);
            ForeignPower _diplomat_civ2 = _diplomat_civ1.GetForeignPower(_civ2);
            string _text;
            bool _writeDirectly = true;
            //  Second.2 = proposalSent
            IProposal proposalSent = _proposalSent;
            if (proposalSent != null)
            {
                _diplomat_civ2.CounterpartyForeignPower.ProposalSent = proposalSent;
                _diplomat_civ2.LastProposalSent = proposalSent;

                _text = "Step_8234:; "
                    + DateTime.Now
                    + " > ProposalSent=   "
                    + proposalSent.Clauses[0].ClauseType.ToString() + " (ProposalSent, "
                    + proposalSent.Clauses.Count + " content) "
                    + "; from " + _diplomat_civ2.Owner.ToString()
                    + "; to; " + _diplomat_civ2.Counterparty.ToString()

                    ;
                if (_writeDirectly)
                    Console.WriteLine(_text);
                //GameLog.Client.DiplomacyDetails.DebugFormat("** ProposalSent becomes Counterparty ProposalReceived [{0}], Counterparty = {1}, Owner = {2}"
                //    , _diplomatForeignPower_Civ2.LastProposalSent.Clauses[0].ClauseType.ToString()
                //    , _diplomatForeignPower_Civ2.Counterparty.ToString(), _diplomatForeignPower_Civ2.Owner.ToString()); ;

                GameContext.Current.CivilizationManagers[_civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(_civ1, proposalSent));

                GameContext.Current.CivilizationManagers[_civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(_civ2, proposalSent));

                _diplomat_civ2.ProposalSent = null;
            }
            else
            {
                Debugger.Break();
                _diplomat_civ2.CounterpartyForeignPower.ProposalReceived = null;
            }
        }

        public static void Diplomacy_9_ConsoleWriteline(Civilization civ1, Civilization civ2)
        {
            Diplomat _diplomatCiv1 = Diplomat.Get(civ1);
            ForeignPower _diplomatCiv2 = _diplomatCiv1.GetForeignPower(civ2);
            //string _newline = Environment.NewLine;
            string _text = "Step_0718:; " + DateTime.Now + " > ";

            _text = "doesn't work well > Diplomacy_9_ConsoleWriteline";


            _text = "#region Gamelogs";
            if (_diplomatCiv2.ProposalReceived != null)
            {
                _text += /*Environment.NewLine + */"ProposalReceived: "
                          + _diplomatCiv2.ProposalReceived.Sender + " to "
                          + _diplomatCiv2.ProposalReceived.Recipient + ": > "
                          + _diplomatCiv2.ProposalReceived.Clauses.ToString()
                          // + Environment.NewLine
                          ;
                Console.WriteLine(_text);
            }

            if (_diplomatCiv2.ProposalSent != null)
            {
                _text += /*Environment.NewLine + */"ProposalSent: "
                          + _diplomatCiv2.ProposalSent.Sender + " to "
                          + _diplomatCiv2.ProposalSent.Recipient + ": > "
                          + _diplomatCiv2.ProposalSent.Clauses.ToString()
                          // + Environment.NewLine
                          ;
                Console.WriteLine(_text);
            }

            if (_diplomatCiv2.ResponseReceived != null)
            {
                _text += /*Environment.NewLine +*/ "ResponseReceived: "
                          + _diplomatCiv2.ResponseReceived.Sender + " to "
                          + _diplomatCiv2.ResponseReceived.Recipient + ": > "
                          + _diplomatCiv2.ResponseReceived.ResponseType.ToString()
                          // + Environment.NewLine
                          ;
                Console.WriteLine(_text);
            }

            if (_diplomatCiv2.ResponseSent != null)
            {
                _text += "ResponseSent: "
                          + _diplomatCiv2.ResponseSent.Sender + " to "
                          + _diplomatCiv2.ResponseSent.Recipient + ": > "
                          + _diplomatCiv2.ResponseSent.ResponseType.ToString()
                          // + Environment.NewLine
                          ;
                Console.WriteLine(_text);
            }

            if (_diplomatCiv2.StatementReceived != null)  // in SinglePlayer you'll never get this "received" because you are always the playing SENDER unitl AI sends
            {

                //string parameterString = _diplomatForeignPower_Civ2.StatementSent.Parameter.ToString() ?? "";

                _text += /*Environment.NewLine + */"StatementReceived: "
                          + _diplomatCiv2.StatementReceived.Sender + " to "
                          + _diplomatCiv2.StatementReceived.Recipient + ": > "
                          + ", Parameter = " //+ parameterString
                          + Enum.GetName(typeof(StatementType), _diplomatCiv2.StatementReceived.StatementType)
                          // + Environment.NewLine
                          ;
                Console.WriteLine(_text);
            }
            if (_diplomatCiv2.StatementSent != null)  // in SinglePlayer you'll never get this "received" because you are always the playing SENDER unitl AI sends
            {

                //string parameterString = _diplomatForeignPower_Civ2.StatementSent.Parameter.ToString() ?? "";

                _text += /*Environment.NewLine + */"StatementSent: "
                          + _diplomatCiv2.StatementSent.Sender + " to "
                          + _diplomatCiv2.StatementSent.Recipient + ": > "
                          + ", Parameter = " //+ parameterString
                                             // + Environment.NewLine
                          ;
                Console.WriteLine(_text);
            }

            // GameLog.Core.Diplomacy.DebugFormat("------------------------------------------");
            //GameLog.Core.DiplomacyDetails.DebugFormat("received a 'Sabotage'-Diplomacy-Statement, Tone = {0}", _diplomatForeignPower_Civ2.StatementReceived.Tone.ToString());

            //if (_text.Length > 44)  // not only the entry phrase...
            //{
            //    Console.WriteLine(/*"Step_0718:; " + DateTime.Now + " > " + */_text);
            //    //GameLog.Core.DiplomacyDetails.DebugFormat(_text);
            //}

            _text = "what's next + ";

            if (_diplomatCiv2.StatementSent != null)
            {
                _text += /*Environment.NewLine + */"(relevant is just the receive on HOSTING side.... StatementSent: "
                            + _diplomatCiv2.StatementSent.Sender + " vs "
                            + _diplomatCiv2.StatementSent.Recipient + ": > "
                            + _diplomatCiv2.StatementSent.StatementType.ToString()
                            + ", Parameter = " //+ parameterString
                                               //+ Environment.NewLine
                            ;
                Console.WriteLine(_text);
            }

            if (_diplomatCiv2.PendingAction != PendingDiplomacyAction.None)
            {
                _text += /*Environment.NewLine + */"PendingAction: "
                            //+ _diplomatForeignPower_Civ2.PendingAction + " vs "
                            //+ _diplomatForeignPower_Civ2.PendingAction.Recipient
                            + _diplomatCiv2.PendingAction.ToString()
                            //+ Environment.NewLine
                            ;
                Console.WriteLine(_text);
            }

        }

        private static string GetEnumString(StatementType value)
        {
            return Enum.GetName(typeof(StatementType), value);
        }

        public static StatementType GetStatementType(bool accepting, Civilization sender, Civilization localPlayerCiv)
        {
            string TrueFalse = "F";
            if (accepting == true)
            {
                TrueFalse = "T";
            }

            string nameOfStatementType = TrueFalse + sender.CivID.ToString() + localPlayerCiv.CivID.ToString();
            switch (nameOfStatementType)
            {
                case "T01":
                    {
                        return StatementType.T01;
                    }
                case "T02":
                    {
                        return StatementType.T02;
                    }
                case "T03":
                    {
                        return StatementType.T03;
                    }
                case "T04":
                    {
                        return StatementType.T04;
                    }
                case "T05":
                    {
                        return StatementType.T05;
                    }
                case "T10":
                    {
                        return StatementType.T10;
                    }
                case "T12":
                    {
                        return StatementType.T12;
                    }
                case "T13":
                    {
                        return StatementType.T13;
                    }
                case "T14":
                    {
                        return StatementType.T14;
                    }
                case "T15":
                    {
                        return StatementType.T15;
                    }
                case "T20":
                    {
                        return StatementType.T20;
                    }
                case "T21":
                    {
                        return StatementType.T21;
                    }
                case "T23":
                    {
                        return StatementType.T23;
                    }
                case "T24":
                    {
                        return StatementType.T24;
                    }
                case "T25":
                    {
                        return StatementType.T25;
                    }
                case "T30":
                    {
                        return StatementType.T30;
                    }
                case "T31":
                    {
                        return StatementType.T31;
                    }
                case "T32":
                    {
                        return StatementType.T32;
                    }
                case "T34":
                    {
                        return StatementType.T34;
                    }
                case "T35":
                    {
                        return StatementType.T35;
                    }
                case "T40":
                    {
                        return StatementType.T40;
                    }
                case "T41":
                    {
                        return StatementType.T41;
                    }
                case "T42":
                    {
                        return StatementType.T42;
                    }
                case "T43":
                    {
                        return StatementType.T43;
                    }
                case "T45":
                    {
                        return StatementType.T45;
                    }
                case "T50":
                    {
                        return StatementType.T50;
                    }
                case "T51":
                    {
                        return StatementType.T51;
                    }
                case "T52":
                    {
                        return StatementType.T52;
                    }
                case "T53":
                    {
                        return StatementType.T53;
                    }
                case "T54":
                    {
                        return StatementType.T54;
                    }
                case "F01":
                    {
                        return StatementType.F01;
                    }
                case "F02":
                    {
                        return StatementType.F02;
                    }
                case "F03":
                    {
                        return StatementType.F03;
                    }
                case "F04":
                    {
                        return StatementType.F04;
                    }
                case "F05":
                    {
                        return StatementType.F05;
                    }
                case "F10":
                    {
                        return StatementType.F10;
                    }
                case "F12":
                    {
                        return StatementType.F12;
                    }
                case "F13":
                    {
                        return StatementType.F13;
                    }
                case "F14":
                    {
                        return StatementType.F14;
                    }
                case "F15":
                    {
                        return StatementType.F15;
                    }
                case "F20":
                    {
                        return StatementType.F20;
                    }
                case "F21":
                    {
                        return StatementType.F21;
                    }
                case "F23":
                    {
                        return StatementType.F23;
                    }
                case "F24":
                    {
                        return StatementType.F24;
                    }
                case "F25":
                    {
                        return StatementType.F25;
                    }
                case "F30":
                    {
                        return StatementType.F30;
                    }
                case "F31":
                    {
                        return StatementType.F31;
                    }
                case "F32":
                    {
                        return StatementType.F32;
                    }
                case "F34":
                    {
                        return StatementType.F34;
                    }
                case "F35":
                    {
                        return StatementType.F35;
                    }
                case "F40":
                    {
                        return StatementType.F40;
                    }
                case "F41":
                    {
                        return StatementType.F41;
                    }
                case "F42":
                    {
                        return StatementType.F42;
                    }
                case "F43":
                    {
                        return StatementType.F43;
                    }
                case "F45":
                    {
                        return StatementType.F45;
                    }
                case "F50":
                    {
                        return StatementType.F50;
                    }
                case "F51":
                    {
                        return StatementType.F51;
                    }
                case "F52":
                    {
                        return StatementType.F52;
                    }
                case "F53":
                    {
                        return StatementType.F53;
                    }
                case "F54":
                    {
                        return StatementType.F54;
                    }
                default:
                    return StatementType.NoStatement;
            }

        }

        public static void AcceptRejectDictionaryFromStatement(Statement _statmentRecieved) // find statement in foreignPower during GameEngine and here creat dictionary entry from it
        {
            int turnNumber = GameContext.Current.TurnNumber;
            StatementType _statementType = _statmentRecieved.StatementType;
            string statementAsString = GetEnumString(_statementType);
            string _civIDs = statementAsString.Substring(1, 2);
            _text = "Step_1338:; for _civIDs=" + _civIDs + " read Statement for Dictionary Value";
            Console.WriteLine(_text);
            //GameLog.Client.Diplomacy.DebugFormat("read Statement for Dictionary Value = {0}, current turn = {1}", _civIDs, turnNumber);
            switch (_statementType)
            {
                case StatementType.T01:
                case StatementType.T02:
                case StatementType.T03:
                case StatementType.T04:
                case StatementType.T05:
                case StatementType.T10:
                case StatementType.T12:
                case StatementType.T13:
                case StatementType.T14:
                case StatementType.T15:
                case StatementType.T20:
                case StatementType.T21:
                case StatementType.T23:
                case StatementType.T24:
                case StatementType.T25:
                case StatementType.T30:
                case StatementType.T31:
                case StatementType.T32:
                case StatementType.T34:
                case StatementType.T35:
                case StatementType.T40:
                case StatementType.T41:
                case StatementType.T42:
                case StatementType.T43:
                case StatementType.T45:
                case StatementType.T50:
                case StatementType.T51:
                case StatementType.T52:
                case StatementType.T53:
                case StatementType.T54:
                    AcceptRejectDictionary(_civIDs, true, turnNumber);
                    break;
                case StatementType.F01:
                case StatementType.F02:
                case StatementType.F03:
                case StatementType.F04:
                case StatementType.F05:
                case StatementType.F10:
                case StatementType.F12:
                case StatementType.F13:
                case StatementType.F14:
                case StatementType.F15:
                case StatementType.F20:
                case StatementType.F21:
                case StatementType.F23:
                case StatementType.F24:
                case StatementType.F25:
                case StatementType.F30:
                case StatementType.F31:
                case StatementType.F32:
                case StatementType.F34:
                case StatementType.F35:
                case StatementType.F40:
                case StatementType.F41:
                case StatementType.F42:
                case StatementType.F43:
                case StatementType.F45:
                case StatementType.F50:
                case StatementType.F51:
                case StatementType.F52:
                case StatementType.F53:
                case StatementType.F54:
                    AcceptRejectDictionary(_civIDs, false, turnNumber); // creat dictionary entry from StatementType
                    break;
                case StatementType.CommendWar:
                case StatementType.DenounceWar:
                case StatementType.WarDeclaration:
                case StatementType.StealCredits:
                case StatementType.StealResearch:
                case StatementType.SabotageFood:
                case StatementType.SabotageEnergy:
                case StatementType.SabotageIndustry:
                    break;
                default:
                    break;
            }
        }

        public static void AcceptRejectDictionary_Clear()
        {
            //if (_acceptRejectDictionary != null)
            _acceptRejectDictionary.Clear();
        }
        public static void AcceptRejectDictionary(ForeignPower foreignPower, bool accepted, int turn)  // called from AI
        {
            //int turnNumber = turn; // in case we need this to time clearing of dictionary - Dictionary<string, Tuple<bool, int>>(); or ValueType is a Class with bool and int.
            string foreignPowerID = foreignPower.CounterpartyID.ToString() + foreignPower.OwnerID.ToString();

            if (_acceptRejectDictionary.ContainsKey(foreignPowerID))
            {
                _ = _acceptRejectDictionary.Remove(foreignPowerID);
                _acceptRejectDictionary.Add(foreignPowerID, accepted);
            }
            else { _acceptRejectDictionary.Add(foreignPowerID, accepted); }

            _text = "Step_1334:; _acceptRejectDicionary.Count=" + _acceptRejectDictionary.Count
                + " > ID=" + foreignPowerID
                ;
            Console.WriteLine(_text);
            //GameLog.Client.DiplomacyDetails.DebugFormat("Turn {0}: _acceptRejectDicionary.Count = {1}, Pair(Counter/Owner) = {2}"
            //    , GameContext.Current.TurnNumber
            //    , _acceptRejectDictionary.Count
            //    , foreignPowerID
            //    );
        }
        public static void AcceptRejectDictionary(string civIDs, bool accepted, int turn) // creat ditionary entry
        {
            //int turnNumber = turn; // in case we need this to time clearing of dictionary - Dictionary<string, Tuple<bool, int>>(); or ValueType is a Class with bool and int.

            if (_acceptRejectDictionary.ContainsKey(civIDs))
            {
                _ = _acceptRejectDictionary.Remove(civIDs);
                _acceptRejectDictionary.Add(civIDs, accepted);
            }
            else { _acceptRejectDictionary.Add(civIDs, accepted); }

            //if (_acceptRejectDictionary != null)
            GameLog.Client.DiplomacyDetails.DebugFormat("Turn {0}: _acceptRejectDicionary.Count = {1}, Pair(Counter/Owner) = {2}"
                , GameContext.Current.TurnNumber
                , _acceptRejectDictionary.Count
                , civIDs);
        }
        public static void BreakAgreement([NotNull] IAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement");
            }

            BreakAgreementVisitor.BreakAgreement(agreement);
        }

        public static IList<Civilization> GetAllies([NotNull] Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            return (from whoElse in GameContext.Current.Civilizations
                    where GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyDefensiveAlliance) ||
                          GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyFullAlliance)
                    select whoElse).ToList();
        }

        public static IList<Civilization> GetMemberCivilizations([NotNull] Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (!who.IsEmpire)
            {
                return EmptyCivilizations;
            }

            return (from whoElse in GameContext.Current.Civilizations
                    where GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyMembership)
                    select whoElse).ToList();
        }
        /// <summary>
        /// retruns the list of civilzations any '_civ1' civilization is in contact with.
        /// </summary>
        /// <param name="who"></param>
        /// <returns>IList<Civilization></returns>
        public static IList<Civilization> GetCivilizationsHavingContact([NotNull] Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            return (from whoElse in GameContext.Current.Civilizations
                    where whoElse != who
                    let diplomacyData = GameContext.Current.DiplomacyData[who, whoElse]
                    where diplomacyData.IsContactMade()
                    select whoElse).ToList();
        }
        // looks like MinElement of Regard.CurrentValue is 'worst enemy' (used to check if minor is allied with your enemy) vs whatever trust is
        // see bool IsAlliedWithWorstEnemy() below
        // RegardEventType is enum of events that appear to alter regard levels
        //ToDo look at old Supremacy code for agent and _diplomat code
        public static Civilization GetWorstEnemy([NotNull] Civilization who)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            int civId = GameContext.Current.DiplomacyData.GetValuesForOwner(who)
                .MinElement(o => o.Regard.CurrentValue)
                .CounterpartyID;

            if (civId != -1)
            {
                return GameContext.Current.Civilizations[civId];
            }

            return null;
        }

        public static bool IsSafeTravelGuaranteed(Civilization traveller, Sector sector)
        {
            if (traveller == null)
            {
                throw new ArgumentNullException("traveller");
            }

            if (sector == null)
            {
                throw new ArgumentNullException("sector");
            }

            Civilization sectorOwner = sector.Owner;
            if (sectorOwner == null)
            {
                sectorOwner = GameContext.Current.SectorClaims.GetOwner(sector.Location);
            }

            if (sectorOwner == null || sectorOwner == traveller)
            {
                return true;
            }

            IDiplomacyData diplomacydata = GameContext.Current.DiplomacyData[traveller, sectorOwner];

            switch (diplomacydata.Status)
            {
                case ForeignPowerStatus.Affiliated:
                case ForeignPowerStatus.OwnerIsMember:
                case ForeignPowerStatus.CounterpartyIsMember:
                case ForeignPowerStatus.CounterpartyIsSubjugated:
                case ForeignPowerStatus.Allied:
                case ForeignPowerStatus.Self:
                    return true;
            }
            GameLog.Core.Diplomacy.DebugFormat("Diplomatic Data status ={0}, traveller ={1} sector owner ={2}, sector Name ={3} owner's homey system ={4}", diplomacydata.Status.ToString(), traveller.Key, sectorOwner.Key, sector.Name, sector.Owner.HomeSystemName.ToString());

            return GameContext.Current.AgreementMatrix.IsAgreementActive(
                traveller,
                sectorOwner,
                ClauseType.TreatyOpenBorders);
        }

        /// <summary>
        /// Whether a given <see cref="Civilization"/> can travel through a particular
        /// <see cref="Sector"/>
        /// </summary>
        /// <param name="traveller"></param>
        /// <param name="sector"></param>
        /// <returns></returns>
        public static bool IsTravelAllowed(Civilization traveller, Sector sector)
        {
            bool travel = true;
            if (traveller == null)
            {
                GameLog.Client.AI.DebugFormat("Null _civ1 for sector ={0} {1}", sector.Name, sector.Location);
                throw new ArgumentNullException("traveller");
            }
            if (sector == null)
            {
                throw new ArgumentNullException("sector");
            }

            //Civilization sectorOwner = sector.Owner;
            //if (sectorOwner == null)
            //{
            //    sectorOwner = GameContext.Current.SectorClaims.GetOwner(sector.Location);
            //}

            //GameLog.Core.Diplomacy.DebugFormat("traveller ={0}, sector location ={1}", traveller.Key, sector.Location);

            return travel;
        }

        /// <summary>
        /// Whether two <see cref="Civilization"/>s are allies
        /// </summary>
        /// <param name="who"></param>
        /// <param name="whoElse"></param>
        /// <returns></returns>
        public static bool AreAllied(Civilization who, Civilization whoElse)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (whoElse == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            return GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyFullAlliance) ||
                   GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyDefensiveAlliance) ||
                   GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyMembership);
        }

        /// <summary>
        /// Whether two <see cref="Civilization"/>s are on friendly terms
        /// </summary>
        /// <param name="who"></param>
        /// <param name="whoElse"></param>
        /// <returns></returns>
        public static bool AreFriendly(Civilization who, Civilization whoElse)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (whoElse == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];

            return diplomacyData != null &&
                   diplomacyData.Status >= ForeignPowerStatus.Friendly;
        }
        /// <summary>
        /// Whether two <see cref="Civilization"/>s are on friendly terms
        /// </summary>
        /// <param name="_civ1"></param>
        /// <param name="_civ2"></param>
        /// <returns></returns>
        public static bool AreNotFriendly(Civilization _civ1, Civilization _civ2)
        {
            if (_civ1 == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (_civ2 == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[_civ1, _civ2];
            return diplomacyData != null &&
                   diplomacyData.Status <= ForeignPowerStatus.Cold;
        }

        /// <summary>
        /// Determines whether two particular <see cref="Civilization"/>s are at war
        /// </summary>
        public static bool Status_AtWar(Civilization who, Civilization whoElse)
        {
            if (who == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (whoElse == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            if (who == whoElse) // && !IsContactMade(_civ1, _civ2))
            {
                return false;
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];

            return diplomacyData.Status == ForeignPowerStatus.AtWar;
        }
        /// <summary>
        /// Determines whether two particular <see cref="Civilization"/>s are in Totalwar
        /// </summary>
        //public static bool AreTotalWar(Civilization _civ1, Civilization _civ2)
        //{
        //    if (_civ1 == null)
        //        throw new ArgumentNullException("_civ1");
        //    if (_civ2 == null)
        //        throw new ArgumentNullException("_civ2");
        //    if (_civ1 == _civ2) // && !IsContactMade(_civ1, _civ2))
        //        return false;
        //    var diplomacyData = GameContext.Current.DiplomacyData[_civ1, _civ2];
        //    return diplomacyData.Status == ForeignPowerStatus.TotalWar;
        //}

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/> is at war with anybody
        /// </summary>
        public static bool IsAtWar(Civilization who)
        {
            return GameContext.Current.DiplomacyData.CountWhere(c => c.Status == ForeignPowerStatus.AtWar) > 0;
        }

        public static bool ArePotentialEnemies(Civilization civ1, Civilization civ2)
        {
            if (civ1 == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (civ2 == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            if (civ1 == civ2)
            {
                return true;
            }

            switch (GetForeignPowerStatus(civ1, civ2))
            {
                case ForeignPowerStatus.AtWar:
                case ForeignPowerStatus.Neutral:
                case ForeignPowerStatus.NoContact:
                    return true;
                default:
                    return false;
            }
        }

        public static bool Status_Neutral(Civilization _civ1, Civilization _civ2)
        {
            if (_civ1 == null)
            {
                throw new ArgumentNullException("_civ1");
            }

            if (_civ2 == null)
            {
                throw new ArgumentNullException("_civ2");
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[_civ1, _civ2];

            return diplomacyData != null &&
                   diplomacyData.Status == ForeignPowerStatus.Neutral;
        }

        /// <summary>
        /// Whether or not given <see cref="Civilization"/> is independent
        /// </summary>
        /// <param name="minorPower"></param>
        /// <returns></returns>
        public static bool IsIndependent([NotNull] Civilization Power)
        {
            if (Power == null)
            {
                throw new ArgumentNullException("minorPower");
            }

            if (Power.IsEmpire)
            {
                return true;
            }

            foreach (Civilization empire in GameContext.Current.Civilizations)
            {
                if (empire.IsEmpire && IsMember(Power, empire))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsMember(Civilization minorPower, Civilization empire)
        {
            if (minorPower == null)
            {
                throw new ArgumentNullException("minorPower");
            }

            if (empire == null)
            {
                throw new ArgumentNullException("empire");
            }

            if (minorPower.IsEmpire || !empire.IsEmpire)
            {
                return false;
            }

            IDiplomacyData diplomacyData = GameContext.Current.DiplomacyData[empire, minorPower];

            return diplomacyData != null &&
                   diplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember;
        }

        public static bool IsAlliedWithWorstEnemy(Civilization enemyOf, Civilization allyOf)
        {
            if (enemyOf == null)
            {
                throw new ArgumentNullException("enemyOf");
            }

            if (allyOf == null)
            {
                throw new ArgumentNullException("allyOf");
            }

            Civilization worstEnemy = GetWorstEnemy(enemyOf);
            if (worstEnemy == null)
            {
                return false;
            }

            // Note: This check will fail (as it should) if 'allyOf' is our worst enemy.
            if (GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf, worstEnemy, ClauseType.TreatyDefensiveAlliance) ||
                GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf, worstEnemy, ClauseType.TreatyFullAlliance))
            {
                return true;
            }

            // Check for alliances with any other civs that we hate as much as our worst enemy (we could have more than one)...

            int worstEnemyRegard = GameContext.Current.DiplomacyData[enemyOf, worstEnemy].Regard.CurrentValue;

            return GameContext.Current.DiplomacyData.GetValuesForOwner(enemyOf).Any(
                o => o.CounterpartyID != allyOf.CivID &&
                     o.Regard.CurrentValue <= worstEnemyRegard &&
                     (GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf.CivID, o.CounterpartyID, ClauseType.TreatyDefensiveAlliance) ||
                      GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf.CivID, o.CounterpartyID, ClauseType.TreatyFullAlliance)));
        }

        public static bool IsTradeEstablished(ICivIdentity firstCiv, ICivIdentity secondCiv)
        {
            AgreementMatrix agreementMatrix = GameContext.Current.AgreementMatrix;

            return agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyOpenBorders) ||
                   //agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
                   agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyAffiliation) ||
                   agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyDefensiveAlliance) ||
                   agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyFullAlliance);
        }

        public static int GetResourceCreditValue(ResourceType resource)
        {
            switch (resource)
            {
                case ResourceType.Deuterium:
                    return 50;
                case ResourceType.Dilithium:
                    return 150;
                case ResourceType.Duranium:
                    return 35;
                default:
                    return 0;
            }
        }

        public static double GetAttitudeVariable(Civilization civ, AttitudeVariable variable)
        {
            return 0.0;
        }

        public static void EnsureContact([NotNull] Civilization firstCiv, [NotNull] Civilization secondCiv, MapLocation location, int contactTurn = 0)
        {
            //SoundPlayer _soundPlayer = null;

            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }

            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }

            if (firstCiv == secondCiv)
            {
                return;
            }

            ForeignPower foreignPower = Diplomat.Get(firstCiv).GetForeignPower(secondCiv);
            ForeignPower ownPower = Diplomat.Get(secondCiv).GetForeignPower(firstCiv);
            if (foreignPower.IsContactMade)
            {
                return;
            }

            int actualContactTurn = contactTurn == 0 ? GameContext.Current.TurnNumber : contactTurn;

            foreignPower.MakeContact(actualContactTurn);

            // Only add sitrep entries if contact was made on the current turn.
            if (GameContext.Current.TurnNumber != actualContactTurn)
            {
                return;
            }

            CivilizationManager firstManager = GameContext.Current.CivilizationManagers[firstCiv];
            CivilizationManager secondManager = GameContext.Current.CivilizationManagers[secondCiv];

            firstManager?.SitRepEntries.Add(new ReportFirstContact(firstCiv, secondCiv, location));

            secondManager?.SitRepEntries.Add(new ReportFirstContact(secondCiv, firstCiv, location));

            //GameLog.Core.Diplomacy.DebugFormat("firstManager.Civilization.Key = {0}, second = {1}", firstManager.Civilization.Key, secondManager.Civilization.Key);
            if (firstManager.Civilization.Key == "BORG")
            {
                foreignPower.DeclareWar();
                firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(firstCiv, secondCiv));
                secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(firstCiv, secondCiv));
                // playing 
                //var soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgWeAreTheBorg.ogg");  // ToDo - not working yet
                //soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgResistanceFutile.flac");
                //_soundPlayer.Play("Resources/SoundFX/TaskForceOrders/BorgWeAreTheBorg.mp3"); // at SitRep "Resistance is fut...."

                ApplyTrustChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Trust.CurrentValue * -1);
                ApplyRegardChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Regard.CurrentValue * -1);

                //GameLog.Core.Diplomacy.DebugFormat("foreignPower = {3}, firstManager.Civilization.Key = {0}, second = {1}, TrustDelta {2}", 
                //    firstManager.Civilization.Key, secondManager.Civilization.Key, trustDelta, foreignPower.DiplomacyData);
            }

            if (secondManager.Civilization.Key == "BORG")
            {
                foreignPower.DeclareWar();
                firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                //var soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgWeAreTheBorg.ogg");  // ToDo - not working yet

                ApplyTrustChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Trust.CurrentValue * -1);
                ApplyRegardChange(secondCiv, firstCiv, ownPower.DiplomacyData.Regard.CurrentValue * -1);

                //GameLog.Core.Diplomacy.DebugFormat("secondManager.Civilization.Key = {0}, first = {1}, TrustDelta {2}", secondManager.Civilization.Key, firstManager.Civilization.Key, trustDelta);
            }

            if (!firstManager.Civilization.IsHuman && ShouldTheyGoToWar(firstCiv, secondCiv))
            {
                foreignPower.DeclareWar();
                firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(firstCiv, secondCiv));
                secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(firstCiv, secondCiv));




                ApplyTrustChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Trust.CurrentValue * -1);
                ApplyRegardChange(secondCiv, firstCiv, ownPower.DiplomacyData.Regard.CurrentValue * -1);
            }
            else if (!secondManager.Civilization.IsHuman && ShouldTheyGoToWar(secondCiv, firstCiv))
            {
                foreignPower.CounterpartyForeignPower.DeclareWar();
                firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                ApplyTrustChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Trust.CurrentValue * -1);
                ApplyRegardChange(secondCiv, firstCiv, ownPower.DiplomacyData.Regard.CurrentValue * -1);
            }
        }

        internal static void PerformFirstContacts(Civilization civilization, MapLocation location)
        {
            HashSet<int> otherCivs = new HashSet<int>();

            IEnumerable<Colony> colonies = from colony in GameContext.Current.Universe.FindAt<Colony>(location)
                                           where colony.OwnerID != civilization.CivID
                                           select colony;

            IEnumerable<Ship> ships = from ship in GameContext.Current.Universe.FindAt<Ship>(location)
                                      where ship.OwnerID != civilization.CivID && !otherCivs.Contains(ship.OwnerID)
                                      select ship;

            IEnumerable<Station> stations = from station in GameContext.Current.Universe.FindAt<Station>(location)
                                            where station.OwnerID != civilization.CivID && !otherCivs.Contains(station.OwnerID)
                                            select station;

            foreach (Colony item in colonies)
            {
                _ = otherCivs.Add(item.OwnerID);
            }

            foreach (Ship item in ships)
            {
                _ = otherCivs.Add(item.OwnerID);
            }

            foreach (Station item in stations)
            {
                _ = otherCivs.Add(item.OwnerID);
            }

            foreach (int otherCiv in otherCivs)
            {
                EnsureContact(civilization, GameContext.Current.Civilizations[otherCiv], location);
            }
        }

        public static bool ShouldTheyGoToWar(Civilization _civ1, Civilization _civ2)
        {

            if (!_civ1.IsHuman)
            {

                bool _are_not_friendly = AreNotFriendly(_civ1, _civ2);
                bool _are_neutral = Status_Neutral(_civ1, _civ2);

                int _civ1_firepower = GameContext.Current.CivilizationManagers[_civ1].FirePowerSpace;
                int _civ2_firepower = GameContext.Current.CivilizationManagers[_civ2].FirePowerSpace;
                int _random = RandomHelper.Random(2);


                //Report_Diplomacy_Situation(_civ1, _civ2, _random, _civ1_firepower, _civ2_firepower);
                // 
                // for this > GalaxyMAP + press ALT+M + look at \Resources\Data\Addon\_diplomacyData.txt

                if (_civ1_firepower * 10 > _civ2_firepower * 11)  // don't go to war with just a small advantage
                {
                    if (!_civ1.Traits.Contains("Peaceful")
                        && _are_not_friendly || _are_neutral
                        && _random == 1
                        )
                    //if (!_civ1.Traits.Contains("Peaceful") 
                    //    && (AreNotFriendly(_civ1, _civ2) || (Status_Neutral(_civ1, _civ2) 
                    //    && RandomHelper.Random(2) == 1)))
                    {

                        _text = "maybe we take out _random (or not)";
                        if (_random == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //public static void Report_Diplomacy_Situation(Civilization _civ1, Civilization _civ2, int _random
        //    , int _civ1_firepower, int _civ2_firepower)
        //{
        //    Diplomat _diplomat1 = Diplomat.Get(_civ1);
        //    //CivilizationManager _civM_1 = GameContext.Current.CivilizationManagers[_civ1];

        //    ForeignPower _diplomatForeignPower_Civ2 = _diplomat1.GetForeignPower(_civ2);
        //    CivilizationManager _civM_2 = GameContext.Current.CivilizationManagers[_civ2];

        //    ForeignPowerStatus _foreignPowerStatus = _diplomat1.GetForeignPower(_civ2).DiplomacyData.Status;

        //    string _are_not_friendly = AreNotFriendly(_civ1, _civ2) ? "Not Friendly" : "AreFriendly";

        //    //if (AreNotFriendly(_civ1, _civ2)) ? _are_not_friendly = "Not Friendly" : _are_not_friendly = "AreFriendly";
        //    //{
        //    //    _are_not_friendly = "Not Friendly";
        //    //}
        //    //else
        //    //{
        //    //    _are_not_friendly = "AreFriendly";
        //    //}
        //    string _are_neutral = Status_Neutral(_civ1, _civ2) ? "Neutral" : "Not Neutral";            

        //    _text = "Step_7466:; " 
        //            + " T= " + _diplomat1.GetForeignPower(_civ2).DiplomacyData.Trust.CurrentValue
        //            + " R= " + _diplomat1.GetForeignPower(_civ2).DiplomacyData.Regard.CurrentValue
        //            + " > " + _civ1
        //            //+ " > " + _civ1
        //            + "= " + _civ1_firepower
        //            + " vs " + _civ2_firepower + " (FirePowerSpace)"
        //            + " for " + _civ2
        //            + " > " + _foreignPowerStatus

        //            + " > " + _are_not_friendly
        //            + " > " + _are_neutral
        //            + " > _random= " + _random
        //            ;


        //    Console.WriteLine(_text);
        //}

        public static bool IsContactMade(Civilization source, Civilization target)
        {
            if (source == null)
            {
                return false;
            }
            //throw new ArgumentNullException("source");
            if (target == null)
            {
                return false;
            }
            // throw new ArgumentNullException("target");

            if (source == target)
            {
                return false;
            }
            //GameLog.Core.Test.DebugFormat("Diplomacy: source = {0} target = {1}",source.Key, target.Key);
            return GameContext.Current.DiplomacyData[source, target].IsContactMade();
        }

        public static bool IsScanBlocked(Civilization source, Sector sector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (sector == null)
            {
                throw new ArgumentNullException("sector");
            }

            if (sector != null && sector.Station != null && source != null)
            {
                return source != sector.Station.Owner;
            }
            return false;
        }

        public static bool IsContactMade(int sourceId, int targetId)
        {
            if (sourceId == targetId)
            {
                return true;
            }

            //if (GameContext.Current.DiplomacyData[sourceId, targetId].IsContactMade() == true)
            //    GameLog.Core.Diplomacy.DebugFormat("Is Contact Made ={0} sourceId ={1} targetID ={2}", GameContext.Current.DiplomacyData[sourceId, targetId].IsContactMade(), sourceId, targetId);

            return GameContext.Current.DiplomacyData[sourceId, targetId].IsContactMade();
        }

        public static bool IsContactMade(this IDiplomacyData diplomacyData)
        {
            if (diplomacyData == null)
            {
                throw new ArgumentNullException("diplomacyData");
            }

            return diplomacyData.ContactTurn != 0;
        }

        public static bool IsFirstContact(Civilization source, Civilization target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (source == target)
            {
                return false;
            }

            return GameContext.Current.DiplomacyData[source, target].ContactDuration == 0;
        }

        //public static int ComputeEndWarValue(Civilization sender, Civilization recipient)
        //{
        //    if (sender == null)
        //    {
        //        throw new ArgumentNullException("sender");
        //    }

        //    if (recipient == null)
        //    {
        //        throw new ArgumentNullException("recipient");
        //    }

        //    return 0;
        //}

        public static int GetInitialMemoryWeight(Civilization civ, MemoryType memoryType)
        {
            return GetInitialMemoryWeight(civ, memoryType, out int maxConcurrentMemories);
        }

        public static int GetInitialMemoryWeight(Civilization civ, MemoryType memoryType, out int maxConcurrentMemories)
        {
            DiplomacyDatabase diplomacyDatabase = GameContext.Current.DiplomacyDatabase;

            if ((GameContext.Current.DiplomacyDatabase.CivilizationProfiles.TryGetValue(civ, out DiplomacyProfile diplomacyProfile) &&
                 diplomacyProfile.MemoryWeights.TryGetValue(memoryType, out RelationshipMemoryWeight memoryWeight)) ||
                diplomacyDatabase.DefaultProfile.MemoryWeights.TryGetValue(memoryType, out memoryWeight))
            {
                maxConcurrentMemories = memoryWeight.MaxConcurrentMemories;
                return memoryWeight.Weight;
            }

            maxConcurrentMemories = 0;
            return 0;
        }
        public static List<Civilization> FindOtherContactedCivsForDeltaRegardTrust(Civilization civDeclaring, Civilization civForDelta)
        {
            List<ForeignPower> foreignPowers = new List<ForeignPower>() { Diplomat.Get(civDeclaring).GetForeignPower(civForDelta) };
            List<CivilizationManager> civilizationManagers = GameContext.Current.CivilizationManagers
                .Where(o => o.Civilization.IsEmpire == true
                && o.Civilization != civForDelta).ToList();
            List<Civilization> civList = new List<Civilization>() { civDeclaring };
            foreach (CivilizationManager aCivManager in civilizationManagers)
            {
                if (IsContactMade(civDeclaring, aCivManager.Civilization))
                {
                    civList.Add(aCivManager.Civilization);
                }
            }
            _ = civList.Remove(civDeclaring);
            //if (civList != null)
            //{        
            //    foreach (var thisCiv in civList)
            //    {
            //        Diplomat _diplomat = Diplomat.Get(thisCiv);
            //        ForeignPower foreignPower = _diplomat.GetForeignPower(civDeclaring);
            //        foreignPowers.Add(foreignPower);
            //    }
            //}
            //foreignPowers.Remove(Diplomat.Get(civDeclaring).GetForeignPower(civForDelta));
            return civList; // can be null
        }

        private class ColonyTargetValues
        {
            public ColonyTargetValues(int distance, int DefenseValue)
            {

                //    //public int Distance { get; set; }
                //    //public int DefenseValue { get; set; }

            }
        }
    }
}
