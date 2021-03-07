// File:DiplomacyHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Combat;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Universe;
using Supremacy.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Web.Services.Description;
using System.Windows;
using System.Windows.Navigation;
using System.Xaml.Schema;

namespace Supremacy.Diplomacy
{
    public static class DiplomacyHelper
    {
        private static readonly IList<Civilization> EmptyCivilizations = new Civilization[0];
        private static CollectionBase<RegardEvent> _regardEvents;
        private static Dictionary<string, bool> _acceptRejectDictionary = new Dictionary<string, bool> { { "98", false } };
       //private static Dictionary<string, Civilization> _warPactDictionary = new Dictionary<string, Civilization> { { "987", GameContext.Current.CivilizationManagers[0].Civilization} };
        public static Civilization _diploScreenSelectedForeignPower;

        public static Civilization DiploScreenSelectedForeignPower
        {
            get
            {
                return _diploScreenSelectedForeignPower;
            }
            set
            {
                _diploScreenSelectedForeignPower = value;
            }
        }

        public static ForeignPowerStatus GetForeignPowerStatus([NotNull] ICivIdentity owner, [NotNull] ICivIdentity counterparty)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (counterparty == null)
                throw new ArgumentNullException("counterparty");

            if (owner.CivID == counterparty.CivID)
                return ForeignPowerStatus.NoContact;
            _regardEvents = new CollectionBase<RegardEvent>();
            return GameContext.Current.DiplomacyData[owner.CivID, counterparty.CivID].Status;
        }
           
        public static void ApplyGlobalTrustChange([NotNull] ICivIdentity civ, int trustDelta) 
        {
            if (civ == null)
                throw new ArgumentNullException("civ");

            var civId = civ.CivID;

            foreach (var diplomat in GameContext.Current.Diplomats)
            {
                if (diplomat.OwnerID == civId)
                    continue;

                var foreignPower = diplomat.GetForeignPower(civ);
                if (foreignPower != null)
                    foreignPower.DiplomacyData.Trust.AdjustCurrent(trustDelta);
            }
        }

        public static void ApplyTrustChange([NotNull] ICivIdentity civ, [NotNull] ICivIdentity otherPower, int trustDelta)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (otherPower == null)
                throw new ArgumentNullException("otherPower");
       
            var diplomat = Diplomat.Get(otherPower);
            var foreignPower = diplomat.GetForeignPower(civ);
            //GameLog.Core.Diplomacy.DebugFormat("BEFORE: civ = {0}, otherPower.CivID = {1}, trustDelta = {2}, diplomat.Owner = {3}, foreignPower.OwnerID =n/v, CurrentTrust =n/v",
            //civ, otherPower.CivID, trustDelta, diplomat.Owner);

            //GameLog.Core.Diplomacy.DebugFormat(
            //    "BEFORE: civ = {0}, otherPower = {1}, trustDelta = {2}, diplomat.Owner = {3}, foreignPower = {4}, CurrentTrust = {5}",
            //    GameContext.Current.CivilizationManagers[civ.CivID].Civilization.ShortName,
            //    GameContext.Current.CivilizationManagers[otherPower.CivID].Civilization.ShortName,
            //    trustDelta, diplomat.Owner,
            //    GameContext.Current.CivilizationManagers[foreignPower.OwnerID].Civilization.ShortName,
            //    foreignPower.DiplomacyData.Trust.CurrentValue);

            if (foreignPower != null)
            {
                foreignPower.DiplomacyData.Trust.AdjustCurrent(trustDelta);
                foreignPower.DiplomacyData.Trust.UpdateAndReset();
                foreignPower.UpdateRegardAndTrustMeters();
            }

            //GameLog.Core.Diplomacy.DebugFormat(
            //    "AFTER : civ = {0}, otherPower = {1}, trustDelta = {2}, diplomat.Owner = {3}, foreignPower = {4}, CurrentTrust = {5}",
            //    GameContext.Current.CivilizationManagers[civ.CivID].Civilization.ShortName,
            //    GameContext.Current.CivilizationManagers[otherPower.CivID].Civilization.ShortName,
            //    trustDelta, diplomat.Owner,
            //    GameContext.Current.CivilizationManagers[foreignPower.OwnerID].Civilization.ShortName,
            //    foreignPower.DiplomacyData.Trust.CurrentValue);
        }
        public static void ApplyRegardChange([NotNull] ICivIdentity civ, [NotNull] ICivIdentity otherPower, int regardDelta)
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            if (otherPower == null)
                throw new ArgumentNullException("otherPower");

            var diplomat = Diplomat.Get(otherPower);
            var foreignPower = diplomat.GetForeignPower(civ);

           // GameLog.Core.Diplomacy.DebugFormat(Environment.NewLine + "   Turn {6};BEFORE: otherPower.CivID=;{1};foreignPower.OwnerID=;{4};regardDelta=;{2};CurrentTrust=;{5};diplomat.Owner=;{3};civ=;{0}" + Environment.NewLine,
           // civ, otherPower.CivID, regardDelta, diplomat.Owner, foreignPower.OwnerID, foreignPower.DiplomacyData.Trust.CurrentValue, GameContext.Current.TurnNumber);

            if (foreignPower != null)
            {
                foreignPower.DiplomacyData.Regard.AdjustCurrent(regardDelta);
                foreignPower.DiplomacyData.Regard.UpdateAndReset();
                foreignPower.UpdateRegardAndTrustMeters();

            }
           // GameLog.Core.Diplomacy.DebugFormat(Environment.NewLine + "   Turn {6};AFTER : otherPower.CivID=;{1};foreignPower.OwnerID=;{4};regardDelta=;{2};CurrentTrust=;{5};diplomat.Owner=;{3};civ=;{0}" + Environment.NewLine,
            //civ, otherPower.CivID, regardDelta, diplomat.Owner, foreignPower.OwnerID, foreignPower.DiplomacyData.Trust.CurrentValue, GameContext.Current.TurnNumber);
        }
        public static void ApplyRegardDecay(RegardEventCategories category, RegardDecay decay)
        {
            for (var i = 0; i < _regardEvents.Count; i++)
            {
                var regardEvent = _regardEvents[i];

                // Regard events with a fixed duration do not decay.
                if (regardEvent.Duration > 0)
                    continue;

                var regard = regardEvent.Regard;
                if (regard == 0)
                {
                    _regardEvents.RemoveAt(i--);
                    continue;
                }

                if (!regardEvent.Type.GetCategories().HasFlag(category))
                    continue;

                if (regard > 0)
                    regard = Math.Max(0, (int)(regard * decay.Positive));
                else
                    regard = Math.Min(0, (int)(regard * decay.Negative));

                if (regard == 0)
                    _regardEvents.RemoveAt(i--);
                else
                    regardEvent.Regard = regard;
            }
        }

        public static Colony GetSeatOfGovernment([NotNull] Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");

            var diplomat = GameContext.Current.Diplomats[who.CivID];
            if (diplomat == null)
                return null;

            return diplomat.SeatOfGovernment;
        }

        public static void SendWarDeclaration([NotNull] Civilization declaringCiv, [NotNull] Civilization targetCiv, Tone tone = Tone.Calm)
        {
            GameLog.Client.Diplomacy.DebugFormat("************** Diplo: SendWarDeclaration...");
            if (declaringCiv == null)
                throw new ArgumentNullException("declaringCiv");
            if (targetCiv == null)
                throw new ArgumentNullException("targetCiv");

            if (declaringCiv == targetCiv)
            {
                GameLog.Core.Diplomacy.ErrorFormat(
                    "Civilization {0} attempted to declare war on itself.",
                    declaringCiv.ShortName);
                
                return;
            }
          
            if (AreAtWar(declaringCiv, targetCiv))
            {
                GameLog.Core.Diplomacy.WarnFormat(
                    "Civilization {0} attempted to declare war on {1}, but they were already at war.",
                    declaringCiv.ShortName,
                    targetCiv.ShortName);

                return;                
            }

            var diplomat = Diplomat.Get(declaringCiv);
            var foreignPower = diplomat.GetForeignPower(targetCiv);

            var proposal = new Statement(declaringCiv, targetCiv, StatementType.WarDeclaration, tone);

            foreignPower.StatementSent = proposal;
            GameLog.Client.Diplomacy.DebugFormat("************** Diplo: SendWarDeclaration sent to ForeignPower...");
            foreignPower.CounterpartyForeignPower.StatementReceived = proposal;
            GameLog.Client.Diplomacy.DebugFormat("************** Diplo: SendWarDeclaration turned to RECEIVED at ForeignPower...");
        }

        public static void SpecificCivAcceptingRejecting([NotNull] StatementType statementType) // read statment type to get civIDs and bool accpet reject
        {
            string statementAsString = GetEnumString(statementType);
            string otherCivID = statementAsString.Substring(1, 1);
            string aCivID = statementAsString.Substring(2, 1);
            string trueFalse = statementAsString.Substring(0, 1);
            int aCivint = Int32.Parse(aCivID);
            int otherCivint = Int32.Parse(otherCivID);
            Civilization aCiv = GameContext.Current.Civilizations[aCivint];
            Civilization otherCiv = GameContext.Current.Civilizations[otherCivint];
            var diplomat = Diplomat.Get(aCiv);
            var foreignPower = diplomat.GetForeignPower(otherCiv);
            bool accepting = false;
            if (trueFalse == "T")
            {
                accepting = true;
            }
       
            if (accepting)
            {
                if (foreignPower.CounterpartyForeignPower.LastProposalSent != null) // aCiv is owner of the foreignpower looking for a ProposalRecieved
                {
                    AcceptProposalVisitor.Visit(foreignPower.CounterpartyForeignPower.LastProposalSent);
                    var civManagers = GameContext.Current.CivilizationManagers;
                    var civ1 = foreignPower.CounterpartyForeignPower.Owner;
                    var civ2 = foreignPower.Owner;



                        civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, foreignPower.ResponseSent));

                        civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, foreignPower.ResponseSent));
               
                    foreignPower.CounterpartyForeignPower.LastProposalSent = null;
                    foreignPower.ResponseSent = null;
                }
            }
            else
            {
                if (foreignPower.CounterpartyForeignPower.LastProposalSent != null) // aCiv is owner of the foreignpower looking for a ProposalRecieved
                {
                    RejectProposalVisitor.Visit(foreignPower.CounterpartyForeignPower.LastProposalSent);
                    var civManagers = GameContext.Current.CivilizationManagers;
                    var civ1 = foreignPower.CounterpartyForeignPower.Owner;
                    var civ2 = foreignPower.Owner;

                    civManagers[civ1].SitRepEntries.Add(new DiplomaticSitRepEntry(civ1, foreignPower.ResponseSent));

                    civManagers[civ2].SitRepEntries.Add(new DiplomaticSitRepEntry(civ2, foreignPower.ResponseSent));

                    foreignPower.CounterpartyForeignPower.LastProposalSent = null;
                    foreignPower.ResponseSent = null;
                }
            }
        }

        public static void AcceptingRejecting([NotNull] ICivIdentity civ) // frind entry in dictionary and send as foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal; or Reject
        {
            if (civ == null)
                throw new ArgumentNullException("civ");
            var aCiv = (Civilization)civ;
            var diplomat = Diplomat.Get(civ);
            
            foreach (var otherCiv in GameContext.Current.Civilizations)
            {
                if (aCiv == otherCiv)
                    continue;
                if (!otherCiv.IsEmpire)
                    continue;
                var foreignPower = diplomat.GetForeignPower(otherCiv);

                bool accepting = false;

                string powerID = foreignPower.CounterpartyID.ToString() + foreignPower.OwnerID.ToString();

                //GameLog.Client.Diplomacy.DebugFormat("Check Dictionar foreignPower.Owner = {0}, counterpary ={1} powerID ={2}"
                //, foreignPower.OwnerID
                //, foreignPower.CounterpartyID
                //, powerID.ToString());
                 
                // AcceptRejectDictionary
                if (_acceptRejectDictionary.ContainsKey(powerID)) // check dictionary with key for bool value to accept reject
                {
                    //GameLog.Client.Diplomacy.DebugFormat("Found it in Dictionary");
                    accepting = _acceptRejectDictionary[powerID];
                    if (accepting)
                    {
                        if (foreignPower.ProposalReceived != null) // aCiv is owner of the foreignpower looking for a ProposalRecieved
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.AcceptProposal;

                            GameLog.Client.Diplomacy.DebugFormat(
                                "## PendingAction: ACCEPT ={0}, Counterparty = {1} Onwer = {2}"
                                , foreignPower.PendingAction.ToString()
                                , foreignPower.Counterparty.ShortName
                                , foreignPower.Owner.ShortName);
                            //if(foreignPower.ProposalReceived != null)
                            //GameLog.Client.Diplomacy.DebugFormat(
                            //   "## ProposlaReceived count={0},  = {1} LastProposalReceived= {2}"
                            //   , foreignPower.ProposalReceived.Clauses.Count()
                            //   , foreignPower.LastProposalReceived.Clauses.Count()
                            //   , foreignPower.Owner.ShortName);
                            //foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                            //foreignPower.ProposalReceived = null;
                            //GameLog.Client.Diplomacy.DebugFormat("LastProposalReceived ={0} on foreignPower.Owner ={1} clause count ={2}"
                            //    , foreignPower.LastProposalReceived.ToString()
                            //    , foreignPower.LastProposalReceived.Clauses.Count()
                            //    );
                        }
                    }
                    else
                    {
                        if (foreignPower.ProposalReceived != null)
                        {
                            foreignPower.PendingAction = PendingDiplomacyAction.RejectProposal;

                            GameLog.Client.Diplomacy.DebugFormat(
                                "## PendingAction: REJECT ={0} reset by clause - regard value, Counterparty = {1} Onwer = {2}",
                                foreignPower.PendingAction.ToString(), foreignPower.Counterparty.ShortName,
                                foreignPower.Owner.ShortName);
                            //foreignPower.LastProposalReceived = foreignPower.ProposalReceived;
                            //foreignPower.ProposalReceived = null;
                        }

                    }

                }
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
                TrueFalse = "T";

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
            var _statementType = _statmentRecieved.StatementType;
            string statementAsString = GetEnumString(_statementType);
            string _civIDs = statementAsString.Substring(1,2);
            GameLog.Client.Diplomacy.DebugFormat("Read Statement for Dictionary Value = {0}, current turn = {1}",_civIDs, turnNumber );
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

        public static void ClearAcceptRejectDictionary()
        {
            //if (_acceptRejectDictionary != null)
                _acceptRejectDictionary.Clear();
        }
        public static void AcceptRejectDictionary(ForeignPower foreignPower, bool accepted, int turn)  // called from AI
        {
            int turnNumber = turn; // in case we need this to time clearing of dictionary - Dictionary<string, Tuple<bool, int>>(); or ValueType is a Class with bool and int.
            string foreignPowerID = foreignPower.CounterpartyID.ToString() + foreignPower.OwnerID.ToString();

            if (_acceptRejectDictionary.ContainsKey(foreignPowerID))
            {
                _acceptRejectDictionary.Remove(foreignPowerID);
                _acceptRejectDictionary.Add(foreignPowerID, accepted);
            }
            else { _acceptRejectDictionary.Add(foreignPowerID, accepted); }

            GameLog.Client.Diplomacy.DebugFormat("Turn {0}: _acceptRejectDicionary.Count = {1}, Pair(Counter/Owner) = {2}"
                , GameContext.Current.TurnNumber
                , _acceptRejectDictionary.Count
                , foreignPowerID
                );
        }
        public static void AcceptRejectDictionary(string civIDs, bool accepted, int turn) // creat ditionary entry
        {
            int turnNumber = turn; // in case we need this to time clearing of dictionary - Dictionary<string, Tuple<bool, int>>(); or ValueType is a Class with bool and int.

            if (_acceptRejectDictionary.ContainsKey(civIDs))
            {
                _acceptRejectDictionary.Remove(civIDs);
                _acceptRejectDictionary.Add(civIDs, accepted);
            }
            else { _acceptRejectDictionary.Add(civIDs, accepted); }

            //if (_acceptRejectDictionary != null)
            GameLog.Client.Diplomacy.DebugFormat("Turn {0}: _acceptRejectDicionary.Count = {1}, Pair(Counter/Owner) = {2}"
                , GameContext.Current.TurnNumber
                , _acceptRejectDictionary.Count
                , civIDs);
        }
        public static void BreakAgreement([NotNull] IAgreement agreement)
        {
            if (agreement == null)
                throw new ArgumentNullException("agreement");

            BreakAgreementVisitor.BreakAgreement(agreement);
        }

        public static IList<Civilization> GetAllies([NotNull] Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");

            return (from whoElse in GameContext.Current.Civilizations
                    where GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyDefensiveAlliance) ||
                          GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyFullAlliance)
                    select whoElse).ToList();
        }

        public static IList<Civilization> GetMemberCivilizations([NotNull] Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");

            if (!who.IsEmpire)
                return EmptyCivilizations;

            return (from whoElse in GameContext.Current.Civilizations
                    where GameContext.Current.AgreementMatrix.IsAgreementActive(who, whoElse, ClauseType.TreatyMembership)
                    select whoElse).ToList();
        }
        /// <summary>
        /// retruns the list of civilzations any 'who' civilization is in contact with.
        /// </summary>
        /// <param name="who"></param>
        /// <returns>IList<Civilization></returns>
        public static IList<Civilization> GetCivilizationsHavingContact([NotNull] Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");

            return (from whoElse in GameContext.Current.Civilizations
                    where whoElse != who
                    let diplomacyData = GameContext.Current.DiplomacyData[who, whoElse]
                    where diplomacyData.IsContactMade()
                    select whoElse).ToList();
        }
        // looks like MinElement of Regard.CurrentValue is 'worst enemy' (used to check if minor is allied with your enemy) vs whatever trust is
        // see bool IsAlliedWithWorstEnemy() below
        // RegardEventType is enum of events that appear to alter regard levels
        //ToDo look at old Supremacy code for agent and diplomat code
        public static Civilization GetWorstEnemy([NotNull] Civilization who)
        {
            if (who == null)
                throw new ArgumentNullException("who");

            var civId = GameContext.Current.DiplomacyData.GetValuesForOwner(who)
                .MinElement(o => o.Regard.CurrentValue)
                .CounterpartyID;

            if (civId != -1)
                return GameContext.Current.Civilizations[civId];

            return null;
        }

        public static bool IsSafeTravelGuaranteed(Civilization traveller, Sector sector)
        {
            if (traveller == null)
                throw new ArgumentNullException("traveller");
            if (sector == null)
                throw new ArgumentNullException("sector");

            var sectorOwner = sector.Owner;
            if (sectorOwner == null)
                sectorOwner = GameContext.Current.SectorClaims.GetOwner(sector.Location);

            if (sectorOwner == null || sectorOwner == traveller)
                return true;

            var diplomacydata = GameContext.Current.DiplomacyData[traveller, sectorOwner];

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
                GameLog.Client.AI.DebugFormat("Null civ for sector ={0} {1}", sector.Name, sector.Location);
                throw new ArgumentNullException("traveller");
            }
            if (sector == null)
                throw new ArgumentNullException("sector");
           
            var sectorOwner = sector.Owner;
            if (sectorOwner == null)
                sectorOwner = GameContext.Current.SectorClaims.GetOwner(sector.Location);

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
                throw new ArgumentNullException("who");
            if (whoElse == null)
                throw new ArgumentNullException("whoElse");

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
                throw new ArgumentNullException("who");
            if (whoElse == null)
                throw new ArgumentNullException("whoElse");

            var diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];

            return diplomacyData != null &&
                   diplomacyData.Status >=ForeignPowerStatus.Friendly;
        }
        /// <summary>
        /// Whether two <see cref="Civilization"/>s are on friendly terms
        /// </summary>
        /// <param name="who"></param>
        /// <param name="whoElse"></param>
        /// <returns></returns>
        public static bool AreNotFriendly(Civilization who, Civilization whoElse)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            if (whoElse == null)
                throw new ArgumentNullException("whoElse");
            var diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];
            return diplomacyData != null &&
                   diplomacyData.Status <= ForeignPowerStatus.Cold;
        }

        /// <summary>
        /// Determines whether two particular <see cref="Civilization"/>s are at war
        /// </summary>
        public static bool AreAtWar(Civilization who, Civilization whoElse)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            if (whoElse == null)
                throw new ArgumentNullException("whoElse");
            if (who == whoElse) // && !IsContactMade(who, whoElse))
                return false;

            var diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];

            return diplomacyData.Status == ForeignPowerStatus.AtWar;
        }
        /// <summary>
        /// Determines whether two particular <see cref="Civilization"/>s are in Totalwar
        /// </summary>
        //public static bool AreTotalWar(Civilization who, Civilization whoElse)
        //{
        //    if (who == null)
        //        throw new ArgumentNullException("who");
        //    if (whoElse == null)
        //        throw new ArgumentNullException("whoElse");
        //    if (who == whoElse) // && !IsContactMade(who, whoElse))
        //        return false;
        //    var diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];
        //    return diplomacyData.Status == ForeignPowerStatus.TotalWar;
        //}

        /// <summary>
        /// Determines whether the given <see cref="Civilization"/> is at war with anybody
        /// </summary>
        public static bool IsAtWar(Civilization who)
        {
            return (GameContext.Current.DiplomacyData.CountWhere(c => c.Status == ForeignPowerStatus.AtWar) > 0);
        }
       
        public static bool ArePotentialEnemies(Civilization civ1, Civilization civ2)
        {
            if (civ1 == null)
                throw new ArgumentNullException("civ1");
            if (civ2 == null)
                throw new ArgumentNullException("civ2");

            if (civ1 == civ2)
                return true;

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

        public static bool AreNeutral(Civilization who, Civilization whoElse)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            if (whoElse == null)
                throw new ArgumentNullException("whoElse");

            var diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];

            return diplomacyData != null &&
                   diplomacyData.Status == ForeignPowerStatus.Neutral;
        }

        /// <summary>
        /// Whether or not given <see cref="Civilization"/> is independent
        /// </summary>
        /// <param name="minorPower"></param>
        /// <returns></returns>
        public static bool IsIndependent([NotNull] Civilization minorPower)
        {
            if (minorPower == null)
                throw new ArgumentNullException("minorPower");

            if (minorPower.IsEmpire)
                return true;

            foreach (var empire in GameContext.Current.Civilizations)
            {
                if (empire.IsEmpire && IsMember(minorPower, empire))
                    return false;
            }

            return true;
        }

        public static bool IsMember(Civilization minorPower, Civilization empire)
        {
            if (minorPower == null)
                throw new ArgumentNullException("minorPower");
            if (empire == null)
                throw new ArgumentNullException("empire");

            if (minorPower.IsEmpire || !empire.IsEmpire)
                return false;

            var diplomacyData = GameContext.Current.DiplomacyData[empire, minorPower];

            return diplomacyData != null &&
                   diplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember;
        }

        public static bool IsAlliedWithWorstEnemy(Civilization enemyOf, Civilization allyOf)
        {
            if (enemyOf == null)
                throw new ArgumentNullException("enemyOf");
            if (allyOf == null)
                throw new ArgumentNullException("allyOf");
            
            var worstEnemy = GetWorstEnemy(enemyOf);
            if (worstEnemy == null)
                return false;

            // Note: This check will fail (as it should) if 'allyOf' is our worst enemy.
            if (GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf, worstEnemy, ClauseType.TreatyDefensiveAlliance) ||
                GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf, worstEnemy, ClauseType.TreatyFullAlliance))
            {
                return true;
            }

            // Check for alliances with any other civs that we hate as much as our worst enemy (we could have more than one)...

            var worstEnemyRegard = GameContext.Current.DiplomacyData[enemyOf, worstEnemy].Regard.CurrentValue;

            return GameContext.Current.DiplomacyData.GetValuesForOwner(enemyOf).Any(
                o => o.CounterpartyID != allyOf.CivID &&
                     o.Regard.CurrentValue <= worstEnemyRegard &&
                     (GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf.CivID, o.CounterpartyID, ClauseType.TreatyDefensiveAlliance) ||
                      GameContext.Current.AgreementMatrix.IsAgreementActive(allyOf.CivID, o.CounterpartyID, ClauseType.TreatyFullAlliance)));
        }

        public static bool IsTradeEstablished(ICivIdentity firstCiv, ICivIdentity secondCiv)
        {
            var agreementMatrix = GameContext.Current.AgreementMatrix;

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
                case ResourceType.RawMaterials:
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
                throw new ArgumentNullException("firstCiv");
            if (secondCiv == null)
                throw new ArgumentNullException("secondCiv");

            if (firstCiv == secondCiv)
                return;

            var foreignPower = Diplomat.Get(firstCiv).GetForeignPower(secondCiv);
            var ownPower = Diplomat.Get(secondCiv).GetForeignPower(firstCiv);
            if (foreignPower.IsContactMade)
                return;

            var actualContactTurn = contactTurn == 0 ? GameContext.Current.TurnNumber : contactTurn;

            foreignPower.MakeContact(actualContactTurn);

            // Only add sitrep entries if contact was made on the current turn.
            if (GameContext.Current.TurnNumber != actualContactTurn)
                return;

            var firstManager = GameContext.Current.CivilizationManagers[firstCiv];
            var secondManager = GameContext.Current.CivilizationManagers[secondCiv];

            if (firstManager != null)
                firstManager.SitRepEntries.Add(new FirstContactSitRepEntry(firstCiv, secondCiv, location));

            if (secondManager != null)
                secondManager.SitRepEntries.Add(new FirstContactSitRepEntry(secondCiv, firstCiv, location));

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
            var otherCivs = new HashSet<int>();

            var colonies = from colony in GameContext.Current.Universe.FindAt<Colony>(location)
                           where colony.OwnerID != civilization.CivID
                           select colony;

            var ships = from ship in GameContext.Current.Universe.FindAt<Ship>(location)
                          where ship.OwnerID != civilization.CivID && !otherCivs.Contains(ship.OwnerID)
                          select ship;

            var stations = from station in GameContext.Current.Universe.FindAt<Station>(location)
                        where station.OwnerID != civilization.CivID && !otherCivs.Contains(station.OwnerID)
                        select station;

            foreach (var item in colonies)
                otherCivs.Add(item.OwnerID);
            foreach (var item in ships)
                otherCivs.Add(item.OwnerID);
            foreach (var item in stations)
                otherCivs.Add(item.OwnerID);

            foreach (var otherCiv in otherCivs)
                EnsureContact(civilization, GameContext.Current.Civilizations[otherCiv], location);
        }

        public static bool ShouldTheyGoToWar(Civilization oneCiv, Civilization twoCiv)
        {
            bool goodDayToDie = false;
            if (!oneCiv.IsHuman)
            {
                if (GameContext.Current.CivilizationManagers[oneCiv].MaintenanceCostLastTurn > GameContext.Current.CivilizationManagers[twoCiv].MaintenanceCostLastTurn)
                {
                    if (oneCiv.Traits.Contains("Warlike") && (AreNotFriendly(oneCiv, twoCiv) || (AreNeutral(oneCiv, twoCiv) && RandomHelper.Random(2)==1)))
                        goodDayToDie = true;
                }
            }
            return goodDayToDie;
        }
        public static bool IsContactMade(Civilization source, Civilization target)
        {
            if (source == null)
                return false;
                //throw new ArgumentNullException("source");
            if (target == null)
                return false;
               // throw new ArgumentNullException("target");

            if (source == target)
                return false;
            //GameLog.Core.Test.DebugFormat("Diplomacy: source = {0} target = {1}",source.Key, target.Key);
            return GameContext.Current.DiplomacyData[source, target].IsContactMade();
        }

        public static bool IsScanBlocked(Civilization source, Sector sector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (sector == null)
                throw new ArgumentNullException("sector");

            if ( sector!= null && sector.Station != null && source != null)
            {
                return source != sector.Station.Owner;
            }
            return false;
        }

        public static bool IsContactMade(int sourceId, int targetId)
        {
            if (sourceId == targetId)
                return true;

            //if (GameContext.Current.DiplomacyData[sourceId, targetId].IsContactMade() == true)
            //    GameLog.Core.Diplomacy.DebugFormat("Is Contact Made ={0} sourceId ={1} targetID ={2}", GameContext.Current.DiplomacyData[sourceId, targetId].IsContactMade(), sourceId, targetId);

            return GameContext.Current.DiplomacyData[sourceId, targetId].IsContactMade();
        }

        public static bool IsContactMade(this IDiplomacyData diplomacyData)
        {
            if (diplomacyData == null)
                throw new ArgumentNullException("diplomacyData");

            return diplomacyData.ContactTurn != 0;
        }

        public static bool IsFirstContact(Civilization source, Civilization target)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            if (source == target)
                return false;

            return GameContext.Current.DiplomacyData[source, target].ContactDuration == 0;
        }

        public static int ComputeEndWarValue(Civilization sender, Civilization recipient)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (recipient == null)
                throw new ArgumentNullException("recipient");

            return 0;
        }

        public static int GetInitialMemoryWeight(Civilization civ, MemoryType memoryType)
        {
            return GetInitialMemoryWeight(civ, memoryType, out int maxConcurrentMemories);
        }

        public static int GetInitialMemoryWeight(Civilization civ, MemoryType memoryType, out int maxConcurrentMemories)
        {
            var diplomacyDatabase = GameContext.Current.DiplomacyDatabase;

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
            List<ForeignPower> foreignPowers = new List<ForeignPower>() {Diplomat.Get(civDeclaring).GetForeignPower(civForDelta)};
            List<CivilizationManager> civilizationManagers = GameContext.Current.CivilizationManagers
                .Where(o => o.Civilization.IsEmpire == true
                && o.Civilization != civForDelta).ToList();
            List<Civilization> civList = new List<Civilization>() { civDeclaring };
            foreach (var aCivManager in civilizationManagers)
            {
                if (IsContactMade(civDeclaring, aCivManager.Civilization))
                {
                    civList.Add(aCivManager.Civilization);
                }
            }
            civList.Remove(civDeclaring);
            //if (civList != null)
            //{        
            //    foreach (var thisCiv in civList)
            //    {
            //        Diplomat diplomat = Diplomat.Get(thisCiv);
            //        ForeignPower foreignPower = diplomat.GetForeignPower(civDeclaring);
            //        foreignPowers.Add(foreignPower);
            //    }
            //}
            //foreignPowers.Remove(Diplomat.Get(civDeclaring).GetForeignPower(civForDelta));
            return civList; // can be null
        }
    }
}
