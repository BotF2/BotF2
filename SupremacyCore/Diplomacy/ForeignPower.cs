// ForeignPower.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;

using System.Linq;

using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Diplomacy
{
    public enum PendingDiplomacyAction
    {
        None,
        AcceptProposal,
        RejectProposal
    }

    [Serializable]
    public class ForeignPower : ICivIdentity, IOwnedDataSerializableAndRecreatable
    {
        private CollectionBase<RegardEvent> _regardEvents;
        private DiplomacyDataInternal _diplomacyData;

        public int OwnerID { get; private set; }
        public int CounterpartyID { get; private set; }
        public bool IsEmbargoInPlace { get; private set; }
        public IProposal ProposalSent { get; set; }
        public IProposal ProposalReceived { get; set; }
        public IProposal LastProposalSent { get; set; }
        public IProposal LastProposalReceived { get; set; }
        public Statement StatementSent { get; set; }
        public Statement StatementReceived { get; set; }
        public Statement LastStatementSent { get; set; }
        public Statement LastStatementReceived { get; set; }
        public IResponse ResponseSent { get; set; }
        public IResponse ResponseReceived { get; set; }
        public IResponse LastResponseSent { get; set; }
        public IResponse LastResponseReceived { get; set; }
        public PendingDiplomacyAction PendingAction { get; set; }

        private string _text;

        //     public bool IsTotalWarInPlace { get; set; }

        public ForeignPower(ICivIdentity owner, ICivIdentity counterparty)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }

            if (counterparty == null)
            {
                throw new ArgumentNullException("counterparty");
            }

            _regardEvents = new CollectionBase<RegardEvent>();
            _diplomacyData = new DiplomacyDataInternal(owner.CivID, counterparty.CivID);

            OwnerID = owner.CivID;
            CounterpartyID = counterparty.CivID;
        }

        public bool IsContactMade => _diplomacyData.IsContactMade();

        public int LastStatusChange => _diplomacyData.LastStatusChange;

        public int TurnsSinceLastStatusChange
        {
            get
            {
                if (!IsContactMade)
                {
                    return 0;
                }

                return GameContext.Current.TurnNumber - LastStatusChange;
            }
        }

        public bool IsDiplomatAvailable
        {
            get
            {
                if (!IsContactMade || DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember || DiplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember)
                {
                    return false;
                }

                if (DiplomacyData.Status != ForeignPowerStatus.AtWar)
                {
                    return true;
                }

                int turnsSinceWarDeclaration = GameContext.Current.TurnNumber - LastStatusChange;
                if (turnsSinceWarDeclaration <= 3)
                {
                    return false;
                }

                return true;
            }
        }

        public IIndexedCollection<RegardEvent> RegardEvents => _regardEvents;

        public DiplomacyDataInternal DiplomacyData => _diplomacyData;

        public IDiplomacyData CounterpartyDiplomacyData => GameContext.Current.DiplomacyData[CounterpartyID, OwnerID];

        public Civilization Owner
        {
            get => GameContext.Current.Civilizations[OwnerID];
            protected set => OwnerID = (value != null) ? value.CivID : Civilization.InvalidID;
        }

        public Civilization Counterparty
        {
            get => GameContext.Current.Civilizations[CounterpartyID];
            protected set => CounterpartyID = (value != null) ? value.CivID : Civilization.InvalidID;
        }

        public ForeignPower CounterpartyForeignPower => GameContext.Current.Diplomats[CounterpartyID].GetForeignPower(Owner);

        public void MakeContact(int contactTurn = 0)
        {
            if (IsContactMade)
            {
                return;
            }

            if (contactTurn == 0)
            {
                contactTurn = GameContext.Current.TurnNumber;
            }

            DiplomacyData.SetContactTurn(contactTurn);
            CounterpartyForeignPower.DiplomacyData.SetContactTurn(contactTurn);

            EnsureCounterpartyTerritoryVisible();
            CounterpartyForeignPower.EnsureCounterpartyTerritoryVisible();

            UpdateStatus();
        }

        private void EnsureCounterpartyTerritoryVisible()
        {
            IIndexedCollection<SectorClaim> claims = GameContext.Current.SectorClaims.GetClaims(CounterpartyID);
            CivilizationMapData mapData = CivilizationManager.For(OwnerID).MapData;

            foreach (SectorClaim claim in claims)
            {
                //GameLog.Core.MapData.DebugFormat("{0}: SetScanned to -> True  for EnsureCounterpartyTerritoryVisible()", claim.Location.ToString());
                mapData.SetScanned(claim.Location, true);
            }
        }

        //public void BeginEmbargo()
        //{
        //    var agreements = GameContext.Current.AgreementMatrix;

        //    foreach (var agreement in agreements)
        //    {
        //        foreach (var clause in agreement.Proposal.Clauses)
        //        {
        //            if (clause.ClauseType == ClauseType.TreatyTradePact)
        //            {
        //                DiplomacyHelper.BreakAgreement(agreement);
        //            }

        //        }
        //    }
        //    // TODO: Break any trade agreements
        //    IsEmbargoInPlace = true;
        //}

        //public void EndEmbargo()
        //{
        //    IsEmbargoInPlace = false;
        //}

        //public void BeginTotalWar()
        //{
        //    List<Civilization> possibleTotalWarCivs = (List<Civilization>)GameContext.Current.Civilizations.Where(o => o.IsEmpire).ToList();
        //    bool foundAlreadyTotalWar = false;
        //    foreach (Civilization civ in possibleTotalWarCivs)
        //    {
        //        var diplomat = Diplomat.Get(civ);
        //        ForeignPower foreignPower = diplomat.GetForeignPower(this.Owner);
        //        if (foreignPower.IsTotalWarInPlace)
        //        {
        //            foundAlreadyTotalWar = true;
        //        }
        //    }
        //    if (foundAlreadyTotalWar == false)
        //    IsTotalWarInPlace = true;
        //}

        //public void EndTotalWar()
        //{
        //    IsTotalWarInPlace = false;
        //}

        public void DeclareWar()
        {
            if (DiplomacyData.Status == ForeignPowerStatus.AtWar)
            {
                return;
            }

            if (DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
            {
                return;
            }

            if (DiplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember)
            {
                return;
            }

            if (!IsContactMade)
            {
                MakeContact();
            }

            IIndexedCollection<IAgreement> activeAgreements = GameContext.Current.AgreementMatrix[OwnerID, CounterpartyID];
            while (activeAgreements.Count > 0)
            {
                BreakAgreementVisitor.BreakAgreement(activeAgreements[0]);
            }

            DiplomacyData.Status = ForeignPowerStatus.AtWar;
            CounterpartyForeignPower.DiplomacyData.Status = ForeignPowerStatus.AtWar;

            if (Owner.Key == "BORG")   // lines above:  Status for Borg is set to AtWar   // every turn, also if Borg make a proposal for friendship
            {
                return; // War is declared at FirstContact, always from the Borg, no option to be declared for the other party
            }

            if (Counterparty.Key == "BORG")   // lines above:  Status for Borg is set to AtWar   // every turn, also if Borg make a proposal for friendship
            {
                return; // War is declared at FirstContact, always from the Borg, no option to be declared for the other party
            }

            Civilization owner = Owner;
            Civilization counterparty = Counterparty;

            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                if (civ == owner ||
                    DiplomacyHelper.IsContactMade(civ, owner) && DiplomacyHelper.IsContactMade(civ, counterparty))
                {
                    GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(
                        new WarDeclaredSitRepEntry(
                            civ,
                            owner,
                            counterparty));
                }
            }
        }

        public void DenounceWar(Civilization victim)
        {
            Civilization owner = Owner;
            Civilization counterparty = Counterparty;
            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                if (civ.IsHuman && (civ == counterparty ||
                    DiplomacyHelper.IsContactMade(civ, owner) && DiplomacyHelper.IsContactMade(civ, counterparty) && DiplomacyHelper.IsContactMade(civ, victim)))
                {
                    GameContext.Current.CivilizationManagers[counterparty].SitRepEntries.Add(
                        new DenounceWarSitRepEntry(
                            owner,
                            counterparty,
                            victim));
                }
            }
        }
        public void CommendWar(Civilization victim)
        {
            Civilization owner = Owner;
            Civilization counterparty = Counterparty;
            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                if (civ.IsHuman && (civ == counterparty ||
                    DiplomacyHelper.IsContactMade(civ, owner) && DiplomacyHelper.IsContactMade(civ, counterparty) && DiplomacyHelper.IsContactMade(civ, victim)))
                {
                    GameContext.Current.CivilizationManagers[counterparty].SitRepEntries.Add(
                        new CommendWarSitRepEntry(
                            owner,
                            counterparty,
                            victim));
                }
            }
        }

        public void ViolateNonAggression(Civilization aggressor)
        {
            if (DiplomacyData.Status == ForeignPowerStatus.AtWar)
            {
                return;
            }

            if (DiplomacyData.Status == ForeignPowerStatus.OwnerIsMember)
            {
                return;
            }

            if (DiplomacyData.Status == ForeignPowerStatus.CounterpartyIsMember)
            {
                return;
            }

            if (!IsContactMade)
            {
                MakeContact();
            }

            IIndexedCollection<IAgreement> activeAgreements = GameContext.Current.AgreementMatrix[OwnerID, CounterpartyID];
            while (activeAgreements.Count > 0)
            {
                BreakAgreementVisitor.BreakAgreement(activeAgreements[0]);
            }

            DiplomacyData.Status = ForeignPowerStatus.Hostile;
            CounterpartyForeignPower.DiplomacyData.Status = ForeignPowerStatus.Hostile;


            Civilization owner = Owner;
            Civilization counterparty = Counterparty;

            foreach (Civilization civ in GameContext.Current.Civilizations)
            {
                if (civ == owner ||
                    DiplomacyHelper.IsContactMade(civ, owner) && DiplomacyHelper.IsContactMade(civ, counterparty))
                {
                    GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(
                        new ViolateTreatySitRepEntry(
                            civ,
                            aggressor,
                            counterparty));
                }
            }
        }

        public void CancelTreaty()
        {
            if (DiplomacyData.Status == ForeignPowerStatus.Neutral)
            {
                return;
            }

            IIndexedCollection<IAgreement> activeAgreements = GameContext.Current.AgreementMatrix[OwnerID, CounterpartyID];
            while (activeAgreements.Count > 0)
            {
                BreakAgreementVisitor.BreakAgreement(activeAgreements[0]);
            }

            DiplomacyData.Status = ForeignPowerStatus.Neutral;
            CounterpartyForeignPower.DiplomacyData.Status = ForeignPowerStatus.Neutral;

            Civilization owner = Owner;
            Civilization counterparty = Counterparty;

            //ToDo sit rep for canceling treaty

            //foreach (var civ in GameContext.Current.Civilizations)
            //{
            //    if (civ == owner ||
            //        DiplomacyHelper.IsContactMade(civ, owner) && DiplomacyHelper.IsContactMade(civ, counterparty))
            //    {
            //        GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(
            //            new WarDeclaredSitRepEntry(
            //                civ,
            //                owner,
            //                counterparty));
            //    }
            //}
        }

        public void AddRegardEvent([NotNull] RegardEvent regardEvent)
        {
            if (regardEvent == null)
            {
                throw new ArgumentNullException("regardEvent");
            }

            _regardEvents.Add(regardEvent);
        }

        public void RemoveRegardEvent([NotNull] RegardEvent regardEvent)
        {
            if (regardEvent == null)
            {
                throw new ArgumentNullException("regardEvent");
            }

            _ = _regardEvents.Remove(regardEvent);
        }

        public void ApplyRegardDecay(RegardEventCategories category, RegardDecay decay)
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

        public void PurgeOldRegardEvents()
        {
            int currentTurn = GameContext.Current.TurnNumber;

            _ = _regardEvents.RemoveWhere(
                e => e.Duration > 0 &&
                     currentTurn - e.Turn >= e.Duration);
        }

        public void UpdateRegardAndTrustMeters()
        {
            PurgeOldRegardEvents();

            Types.Meter regardMeter = DiplomacyData.Regard;

            regardMeter.SaveCurrentAndResetToBase();

            foreach (RegardEvent regardEvent in _regardEvents)
            {
                //GameLog.Client.Diplomacy.DebugFormat("### regardEvent regard ={0}, turn ={1} Type ={2} duration ={3}",
                //    regardEvent.Regard, regardEvent.Turn, regardEvent.Type.ToString(), regardEvent.Duration);

                _ = regardMeter.AdjustCurrent(regardEvent.Regard);

                //GameLog.Client.Diplomacy.DebugFormat("### Regard ={0} Owner ={1} CounterParty ={2}",
                //    DiplomacyData.Regard,
                //    GameContext.Current.CivilizationManagers[OwnerID].Civilization.ShortName,
                //    GameContext.Current.CivilizationManagers[CounterpartyID].Civilization.ShortName);
            }

        }

        protected void ResolveStatus(out ForeignPowerStatus ownerStatus, out ForeignPowerStatus counterpartyStatus)
        {
            if (!IsContactMade)
            {
                ownerStatus = ForeignPowerStatus.NoContact;
                counterpartyStatus = ForeignPowerStatus.NoContact;
                return;
            }

            Civilization owner = Owner;
            Civilization counterparty = Counterparty;
            AgreementMatrix agreementMatrix = GameContext.Current.AgreementMatrix;

            if (agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyMembership))
            {
                ownerStatus = owner.IsEmpire ? ForeignPowerStatus.CounterpartyIsMember : ForeignPowerStatus.OwnerIsMember;
                counterpartyStatus = owner.IsEmpire ? ForeignPowerStatus.OwnerIsMember : ForeignPowerStatus.CounterpartyIsMember;
                return;
            }

            if (agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyDefensiveAlliance) ||
                agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyFullAlliance))
            {
                ownerStatus = ForeignPowerStatus.Allied;
                counterpartyStatus = ForeignPowerStatus.Allied;
                return;
            }

            if (agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyAffiliation))
            {
                ownerStatus = ForeignPowerStatus.Affiliated;
                counterpartyStatus = ForeignPowerStatus.Affiliated;
                return;
            }

            if (
            //agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyTradePact) ||
            //    agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyResearchPact) ||
                agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyOpenBorders))
            {
                ownerStatus = ForeignPowerStatus.Friendly;
                counterpartyStatus = ForeignPowerStatus.Friendly;
                return;
            }

            if (agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyNonAggression))
            {
                ownerStatus = ForeignPowerStatus.Peace;
                counterpartyStatus = ForeignPowerStatus.Peace;
                return;
            }

            if (agreementMatrix.IsAgreementActive(owner, counterparty, ClauseType.TreatyCeaseFire))
            {
                ownerStatus = ForeignPowerStatus.Neutral;
                counterpartyStatus = ForeignPowerStatus.Neutral;
                return;
            }

            UniverseObjectList<Colony> ownerColonies = GameContext.Current.CivilizationManagers[owner].Colonies;
            if (ownerColonies.Count == 0)
            {
                bool anySubjugatedColonies = GameContext.Current.Universe.FindOwned<Colony>(counterparty).Any(o => o.OriginalOwner == owner);
                if (anySubjugatedColonies)
                {
                    ownerStatus = ForeignPowerStatus.OwnerIsSubjugated;
                    counterpartyStatus = ForeignPowerStatus.CounterpartyIsSubjugated;
                    return;
                }
                ownerStatus = ForeignPowerStatus.OwnerIsUnreachable;
                counterpartyStatus = ForeignPowerStatus.CounterpartyIsUnreachable;
                return;
            }

            UniverseObjectList<Colony> counterpartyColonies = GameContext.Current.CivilizationManagers[counterparty].Colonies;
            if (counterpartyColonies.Count == 0)
            {
                bool anySubjugatedColonies = GameContext.Current.Universe.FindOwned<Colony>(owner).Any(o => o.OriginalOwner == counterparty);
                if (anySubjugatedColonies)
                {
                    ownerStatus = ForeignPowerStatus.CounterpartyIsSubjugated;
                    counterpartyStatus = ForeignPowerStatus.OwnerIsSubjugated;
                    return;
                }
                ownerStatus = ForeignPowerStatus.CounterpartyIsUnreachable;
                counterpartyStatus = ForeignPowerStatus.OwnerIsUnreachable;
                return;
            }

            if (DiplomacyData.Status == ForeignPowerStatus.AtWar)
            {
                ownerStatus = ForeignPowerStatus.AtWar;
                counterpartyStatus = ForeignPowerStatus.AtWar;
                return;
            }

            ownerStatus = ForeignPowerStatus.Neutral;
            counterpartyStatus = ForeignPowerStatus.Neutral;
        }

        public void UpdateStatus()
        {

            ResolveStatus(out ForeignPowerStatus ownerStatus, out ForeignPowerStatus counterpartyStatus);

            if (DiplomacyData.Status != ownerStatus)
            {
                DiplomacyData.LastStatusChange = GameContext.Current.TurnNumber;
                DiplomacyData.Status = ownerStatus;
            }

            ForeignPower counterpartyForeignPower = CounterpartyForeignPower;
            if (counterpartyForeignPower.DiplomacyData.Status != counterpartyStatus)
            {
                counterpartyForeignPower.DiplomacyData.LastStatusChange = GameContext.Current.TurnNumber;
                counterpartyForeignPower.DiplomacyData.Status = counterpartyStatus;
            }
        }

        //public void AcceptingRejecting(string acceptReject) 
        //{
        //    bool accepting = false;
        //    if (acceptReject == "ACCEPT")
        //        accepting = true;

        //    if (accepting)
        //    {
        //        PendingAction = PendingDiplomacyAction.AcceptProposal;
        //    }
        //    else
        //    {
        //        PendingAction = PendingDiplomacyAction.RejectProposal;
        //    }
        //    LastProposalReceived = ProposalReceived;
        //    ProposalReceived = null;
        //}

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID => CounterpartyID;

        #endregion

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _regardEvents = reader.Read<CollectionBase<RegardEvent>>();
            _diplomacyData = reader.Read<DiplomacyDataInternal>();

            OwnerID = reader.ReadOptimizedInt32();
            CounterpartyID = reader.ReadOptimizedInt32();
            IsEmbargoInPlace = reader.ReadBoolean();
            ProposalSent = reader.Read<IProposal>();
            ProposalReceived = reader.Read<IProposal>();
            LastProposalSent = reader.Read<IProposal>();
            LastProposalReceived = reader.Read<IProposal>();
            StatementSent = reader.Read<Statement>();
            StatementReceived = reader.Read<Statement>();
            LastStatementSent = reader.Read<Statement>();
            LastStatementReceived = reader.Read<Statement>();
            ResponseSent = reader.Read<Response>();
            ResponseReceived = reader.Read<Response>();
            LastResponseSent = reader.Read<Response>();
            LastResponseReceived = reader.Read<Response>();
            PendingAction = (PendingDiplomacyAction)reader.ReadOptimizedInt32();
            //IsTotalWarInPlace = reader.ReadBoolean();
            _text = "reading ";
            _text += "OwnerID=" + OwnerID + " vs " + CounterpartyID

                + ", _regardEv.Count=" + _regardEvents.Count
                + ", _dipDate=NOT DONE" 
                + ", Psent=" + ProposalSent
                + ", Preceiv=" + ProposalReceived
                + ", LPsent=" + LastProposalSent
                + ", LPr=" + LastProposalSent

                + ", STsent=" + StatementSent
                + ", STreceiv=" + StatementReceived
                + ", LSTsent=" + LastStatementSent
                + ", LSTreceiv=" + LastStatementReceived

                + ", Rsent=" + ResponseSent
                + ", Rreceiv=" + ResponseReceived
                + ", LRsent=" + LastResponseSent
                + ", LRPreceiv=" + LastResponseReceived
                + ", Pending=" + PendingAction
                ;
            foreach (var item in _regardEvents)
            {
                _text += item.Turn;
            }

            //Console.WriteLine(_text);
            GameLog.Client.SaveLoadDetails.DebugFormat(_text);
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            // GameLog.Client.Diplomacy.DebugFormat("SerializeOwnedData ....");
            writer.WriteObject(_regardEvents);
            writer.WriteObject(_diplomacyData);

            writer.WriteOptimized(OwnerID);
            writer.WriteOptimized(CounterpartyID);
            writer.Write(IsEmbargoInPlace);
            writer.WriteObject(ProposalSent);
            writer.WriteObject(ProposalReceived);
            writer.WriteObject(LastProposalSent);
            writer.WriteObject(LastProposalReceived);
            writer.WriteObject(StatementSent);
            writer.WriteObject(StatementReceived);
            writer.WriteObject(LastStatementSent);
            writer.WriteObject(LastStatementReceived);
            writer.WriteObject(ResponseSent);
            writer.WriteObject(ResponseReceived);
            writer.WriteObject(LastResponseSent);
            writer.WriteObject(LastResponseReceived);
            writer.WriteOptimized((int)PendingAction);
            //writer.Write(IsTotalWarInPlace);
        }
    }
}