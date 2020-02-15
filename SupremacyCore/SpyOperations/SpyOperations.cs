// SpyOperations.cs  // former ForeignPower.cs for Diplo
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
using Supremacy.Diplomacy;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;



// plan is: collecting SpyOperations from all clients, doing it here and re-distribute it with usual game context at end of turn processing

namespace Supremacy.SpyOperations
{
    public enum  SpyActionExecute  //PendingSpyAction_NotUsed
    {
        DoItSo,
        Done

        //StealCredits,   // in Diplo: Accept
        //SabotageEnergy,  // In Diplo: Reject
    }

    [Serializable]
    public class Spy_2_Power : ICivIdentity, IOwnedDataSerializableAndRecreatable
    {
        private CollectionBase<RegardEvent> _regardEvents;
        private DiplomacyDataInternal _diplomacyData;

        public int AttackerID { get; private set; }
        public int VictimID { get; private set; }
        //public bool IsEmbargoInPlace { get; private set; }
        //public ISpyOrder ProposalSent { get; set; }
        public ISpyOrder SpyOrderReceived { get; set; }

        //public ISpyOrder LastProposalSent { get; set; }
        //public ISpyOrder LastSpyOrderReceived { get; set; }
        //public Statement StatementSent { get; set; }
        //public Statement StatementReceived { get; set; }
        //public Statement LastStatementSent { get; set; }
        //public Statement LastStatementReceived { get; set; }
        //public IResponse ResponseSent { get; set; }
        //public IResponse ResponseReceived { get; set; }
        //public IResponse LastResponseSent { get; set; }
        //public IResponse LastResponseReceived { get; set; }
        public SpyActionExecute PendingSpyAction { get; set; }

        public Spy_2_Power(ICivIdentity attacker, ICivIdentity victim)
        {
            if (attacker == null)
                throw new ArgumentNullException("attacker");
            if (victim == null)
                throw new ArgumentNullException("victim");

            _regardEvents = new CollectionBase<RegardEvent>();
            _diplomacyData = new DiplomacyDataInternal(attacker.CivID, victim.CivID);

            AttackerID = attacker.CivID;
            VictimID = victim.CivID;
        }

        //public bool IsContactMade
        //{
        //    get { return _diplomacyData.IsContactMade(); }
        //}

        //public int LastStatusChange
        //{
        //    get { return _diplomacyData.LastStatusChange; }
        //}

        //public int TurnsSinceLastStatusChange
        //{
        //    get
        //    {
        //        if (!IsContactMade)
        //            return 0;
        //        return GameContext.Current.TurnNumber - LastStatusChange;
        //    }
        //}

        //public bool IsDiplomatAvailable
        //{
        //    get
        //    {
        //        if (!IsContactMade)
        //            return false;

        //        //if (DiplomacyData.Status != Spy_2_SpyStatus.AtWar)
        //        //    return true;

        //        var turnsSinceWarDeclaration = GameContext.Current.TurnNumber - LastStatusChange;
        //        if (turnsSinceWarDeclaration <= 3)
        //            return false;

        //        return true;
        //    }
        //}

        public IIndexedCollection<RegardEvent> RegardEvents
        {
            get { return _regardEvents; }
        }

        public DiplomacyDataInternal DiplomacyData
        {
            get { return _diplomacyData; }
        }

        public IDiplomacyData VictimDiplomacyData
        {
            get { return GameContext.Current.DiplomacyData[VictimID, AttackerID]; }
        }

        public Civilization Attacker
        {
            get { return GameContext.Current.Civilizations[AttackerID]; }
            protected set { AttackerID = (value != null) ? value.CivID : Civilization.InvalidID; }
        }

        public Civilization Victim
        {
            get { return GameContext.Current.Civilizations[VictimID]; }
            protected set { VictimID = (value != null) ? value.CivID : Civilization.InvalidID; }
        }

        //public Spy_2_Power VictimSpy_2_Power
        //{
        //    get 
        //    { 

        //        return GameContext.Current.Diplomats[VictimID].GetSpy_2_Power(Attacker); 
        //    }
        //}

        //public void MakeContact(int contactTurn = 0)
        //{
        //    if (IsContactMade)
        //        return;

        //    if (contactTurn == 0)
        //        contactTurn = GameContext.Current.TurnNumber;

        //    DiplomacyData.SetContactTurn(contactTurn);
        //    VictimSpy_2_Power.DiplomacyData.SetContactTurn(contactTurn);

        //    EnsureVictimTerritoryVisible();
        //    VictimSpy_2_Power.EnsureVictimTerritoryVisible();

        //    UpdateStatus();
        //}

        //private void EnsureVictimTerritoryVisible()
        //{
        //    var claims = GameContext.Current.SectorClaims.GetClaims(VictimID);
        //    var mapData = CivilizationManager.For(AttackerID).MapData;

        //    foreach (var claim in claims)
        //    {
        //        //GameLog.Core.MapData.DebugFormat("{0}: SetScanned to -> True  for EnsureVictimTerritoryVisible()", claim.Location.ToString());
        //        mapData.SetScanned(claim.Location, true);
        //    }
        //}

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

        public void DeclareWar()
        {
            //if (DiplomacyData.Status == Spy_2_SpyStatus.AtWar)
            //    return;

            //if (!IsContactMade)
            //    MakeContact();

            var activeAgreements = GameContext.Current.AgreementMatrix[AttackerID, VictimID];
            while (activeAgreements.Count > 0)
                BreakAgreementVisitor.BreakAgreement(activeAgreements[0]);

            //DiplomacyData.Status = Spy_2_SpyStatus.AtWar;
            //VictimSpy_2_Power.DiplomacyData.Status = Spy_2_SpyStatus.AtWar;

            if (Attacker.Key == "BORG")   // lines above:  Status for Borg is set to AtWar   // every turn, also if Borg make a proposal for friendship
                return; // War is declared at FirstContact, always from the Borg, no option to be declared for the other party

            if (Victim.Key == "BORG")   // lines above:  Status for Borg is set to AtWar   // every turn, also if Borg make a proposal for friendship
                return; // War is declared at FirstContact, always from the Borg, no option to be declared for the other party

            var attacker = Attacker;
            var victim = Victim;

            foreach (var civ in GameContext.Current.Civilizations)
            {
                if (civ == attacker ||
                    DiplomacyHelper.IsContactMade(civ, attacker) && DiplomacyHelper.IsContactMade(civ, victim))
                {
                    GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(
                        new WarDeclaredSitRepEntry(
                            civ,
                            attacker,
                            victim));
                }
            }
        }

        //public void CancelTreaty()
        //{
        //    //if (DiplomacyData.Status == Spy_2_SpyStatus.Neutral)
        //    //    return;

        //    var activeAgreements = GameContext.Current.AgreementMatrix[AttackerID, VictimID];
        //    while (activeAgreements.Count > 0)
        //        BreakAgreementVisitor.BreakAgreement(activeAgreements[0]);

        //    //DiplomacyData.Status = Spy_2_SpyStatus.Neutral;
        //    //VictimSpy_2_Power.DiplomacyData.Status = Spy_2_SpyStatus.Neutral;

        //    var attacker = Attacker;
        //    var victim = Victim;

        //    //ToDo sit rep for canceling treaty

        //    //foreach (var civ in GameContext.Current.Civilizations)
        //    //{
        //    //    if (civ == attacker ||
        //    //        DiplomacyHelper.IsContactMade(civ, attacker) && DiplomacyHelper.IsContactMade(civ, victim))
        //    //    {
        //    //        GameContext.Current.CivilizationManagers[civ].SitRepEntries.Add(
        //    //            new WarDeclaredSitRepEntry(
        //    //                civ,
        //    //                attacker,
        //    //                victim));
        //    //    }
        //    //}
        //}

        public void AddRegardEvent([NotNull] RegardEvent regardEvent)
        {
            if (regardEvent == null)
                throw new ArgumentNullException("regardEvent");

            _regardEvents.Add(regardEvent);
        }

        public void RemoveRegardEvent([NotNull] RegardEvent regardEvent)
        {
            if (regardEvent == null)
                throw new ArgumentNullException("regardEvent");

            _regardEvents.Remove(regardEvent);
        }

        public void ApplyRegardDecay(RegardEventCategories category, RegardDecay decay)
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

        public void PurgeOldRegardEvents()
        {
            var currentTurn = GameContext.Current.TurnNumber;

            _regardEvents.RemoveWhere(
                e => e.Duration > 0 &&
                     currentTurn - e.Turn >= e.Duration);
        }

        public void UpdateRegardAndTrustMeters()
        {
            PurgeOldRegardEvents();

            var regardMeter = DiplomacyData.Regard;

            regardMeter.SaveCurrentAndResetToBase();

            foreach (var regardEvent in _regardEvents)
                regardMeter.AdjustCurrent(regardEvent.Regard);
        }

        //protected void ResolveStatus(out Spy_2_SpyStatus attackerStatus, out Spy_2_SpyStatus victimStatus)
        //{
        //    //if (!IsContactMade)
        //    //{
        //    //    //attackerStatus = Spy_2_SpyStatus.NoContact;
        //    //    //victimStatus = Spy_2_SpyStatus.NoContact;
        //    //    return;
        //    //}

        //    var attacker = Attacker;
        //    var victim = Victim;
        //    //var agreementMatrix = GameContext.Current.AgreementMatrix;

        //    //if (agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyMembership))
        //    //{
        //    //    //attackerStatus = attacker.IsEmpire ? Spy_2_SpyStatus.VictimIsMember : Spy_2_SpyStatus.AttackerIsMember;
        //    //    //victimStatus = attacker.IsEmpire ? Spy_2_SpyStatus.AttackerIsMember : Spy_2_SpyStatus.VictimIsMember;
        //    //    return;
        //    //}

        //    //if (agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyDefensiveAlliance) ||
        //    //    agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyFullAlliance))
        //    //{
        //    //    //attackerStatus = Spy_2_SpyStatus.Allied;
        //    //    //victimStatus = Spy_2_SpyStatus.Allied;
        //    //    return;
        //    //}

        //    //if (agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyAffiliation))
        //    //{
        //    //    //attackerStatus = Spy_2_SpyStatus.Affiliated;
        //    //    //victimStatus = Spy_2_SpyStatus.Affiliated;
        //    //    return;
        //    //}

        //    //if (agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyTradePact) ||
        //    //    agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyResearchPact) ||
        //    //    agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyOpenBorders))
        //    //{
        //    //    //attackerStatus = Spy_2_SpyStatus.Friendly;
        //    //    //victimStatus = Spy_2_SpyStatus.Friendly;
        //    //    return;
        //    //}

        //    //if (agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyNonAggression))
        //    //{
        //    //    //attackerStatus = Spy_2_SpyStatus.Peace;
        //    //    //victimStatus = Spy_2_SpyStatus.Peace;
        //    //    return;
        //    //}

        //    //if (agreementMatrix.IsAgreementActive(attacker, victim, ClauseType.TreatyCeaseFire))
        //    //{
        //    //    //attackerStatus = Spy_2_SpyStatus.Neutral;
        //    //    //victimStatus = Spy_2_SpyStatus.Neutral;
        //    //    return;
        //    //}

        //    //var attackerColonies = GameContext.Current.CivilizationManagers[attacker].Colonies;
        //    //if (attackerColonies.Count == 0)
        //    //{
        //    //    var anySubjugatedColonies = GameContext.Current.Universe.FindOwned<Colony>(victim).Any(o => o.OriginalAttacker == attacker);
        //    //    if (anySubjugatedColonies)
        //    //    {
        //    //        //attackerStatus = Spy_2_SpyStatus.AttackerIsSubjugated;
        //    //        //victimStatus = Spy_2_SpyStatus.VictimIsSubjugated;
        //    //        return;
        //    //    }
        //    //    //attackerStatus = Spy_2_SpyStatus.AttackerIsUnreachable;
        //    //    //victimStatus = Spy_2_SpyStatus.VictimIsUnreachable;
        //    //    return;
        //    //}



        //    //var victimColonies = GameContext.Current.CivilizationManagers[victim].Colonies;
        //    //if (victimColonies.Count == 0)
        //    //{
        //    //    var anySubjugatedColonies = GameContext.Current.Universe.FindOwned<Colony>(attacker).Any(o => o.OriginalAttacker == victim);
        //    //    if (anySubjugatedColonies)
        //    //    {
        //    //        //attackerStatus = Spy_2_SpyStatus.VictimIsSubjugated;
        //    //        //victimStatus = Spy_2_SpyStatus.AttackerIsSubjugated;
        //    //        return;
        //    //    }
        //    //    //attackerStatus = Spy_2_SpyStatus.VictimIsUnreachable;
        //    //    //victimStatus = Spy_2_SpyStatus.AttackerIsUnreachable;
        //    //    return;
        //    //}




        //    //if (DiplomacyData.Status == Spy_2_SpyStatus.AtWar)
        //    //{
        //    //    attackerStatus = Spy_2_SpyStatus.AtWar;
        //    //    victimStatus = Spy_2_SpyStatus.AtWar;
        //    //    return;
        //    //}

        //    //attackerStatus = Spy_2_SpyStatus.Neutral;
        //    //victimStatus = Spy_2_SpyStatus.Neutral;
        //}

        //public void UpdateStatus()
        //{
        //    Spy_2_SpyStatus attackerStatus;
        //    Spy_2_SpyStatus victimStatus;

        //    ResolveStatus(out attackerStatus, out victimStatus);

        //    //if (DiplomacyData.Status != attackerStatus)
        //    //{
        //    //    DiplomacyData.LastStatusChange = GameContext.Current.TurnNumber;
        //    //    DiplomacyData.Status = attackerStatus;
        //    //}

        //    var victimSpy_2_Power = VictimSpy_2_Power;
        //    //if (victimSpy_2_Power.DiplomacyData.Status != victimStatus)
        //    //{
        //    //    victimSpy_2_Power.DiplomacyData.LastStatusChange = GameContext.Current.TurnNumber;
        //    //    victimSpy_2_Power.DiplomacyData.Status = victimStatus;
        //    //}
        //}

        #region Implementation of ICivIdentity

        int ICivIdentity.CivID
        {
            get { return VictimID; }
        }

        #endregion

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _regardEvents = reader.Read<CollectionBase<RegardEvent>>();
            _diplomacyData = reader.Read<DiplomacyDataInternal>();

            AttackerID = reader.ReadOptimizedInt32();
            VictimID = reader.ReadOptimizedInt32();
            //IsEmbargoInPlace = reader.ReadBoolean();
            //ProposalSent = reader.Read<ISpyOrder>();
            SpyOrderReceived = reader.Read<ISpyOrder>();
            //LastProposalSent = reader.Read<ISpyOrder>();
            //LastSpyOrderReceived = reader.Read<ISpyOrder>();
            //StatementSent = reader.Read<Statement>();
            //StatementReceived = reader.Read<Statement>();
            //LastStatementSent = reader.Read<Statement>();
            //LastStatementReceived = reader.Read<Statement>();
            //ResponseSent = reader.Read<Response>();
            //ResponseReceived = reader.Read<Response>();
            //LastResponseSent = reader.Read<Response>();
            //LastResponseReceived = reader.Read<Response>();
            PendingSpyAction = (SpyActionExecute)reader.ReadOptimizedInt32();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteObject(_regardEvents);
            writer.WriteObject(_diplomacyData);

            writer.WriteOptimized(AttackerID);
            writer.WriteOptimized(VictimID);
            //writer.Write(IsEmbargoInPlace);
            //writer.WriteObject(ProposalSent);
            writer.WriteObject(SpyOrderReceived);
            //writer.WriteObject(LastProposalSent);
            //writer.WriteObject(LastSpyOrderReceived);
            //writer.WriteObject(StatementSent);
            //writer.WriteObject(StatementReceived);
            //writer.WriteObject(LastStatementSent);
            //writer.WriteObject(LastStatementReceived);
            //writer.WriteObject(ResponseSent);
            //writer.WriteObject(ResponseReceived);
            //writer.WriteObject(LastResponseSent);
            //writer.WriteObject(LastResponseReceived);
            writer.WriteOptimized((int)PendingSpyAction);
        }
    }
}