// DiplomacyHelper.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Annotations;
using Supremacy.Collections;
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

namespace Supremacy.Diplomacy
{
    public static class DiplomacyHelper
    {
        private static readonly IList<Civilization> EmptyCivilizations = new Civilization[0];
        

        public static ForeignPowerStatus GetForeignPowerStatus([NotNull] ICivIdentity owner, [NotNull] ICivIdentity counterparty)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (counterparty == null)
                throw new ArgumentNullException("counterparty");

            if (owner.CivID == counterparty.CivID)
                return ForeignPowerStatus.NoContact;

            return GameContext.Current.DiplomacyData[owner.CivID, counterparty.CivID].Status;
        }
            // ToDo look at bringin diplomatic ships to use the old diplomatic code and set up the Civilization-to-diplomat map for games (GameContext.Current.Diplomats)
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

            var civId = civ.CivID;

            foreach (var diplomat in GameContext.Current.Diplomats)
            {
                //GameLog.Core.Diplomacy.DebugFormat("BEFORE: civ = {0}, otherPower.CivID = {1}, trustDelta = {2}, diplomat.Owner = {3}, foreignPower.OwnerID =n/v, CurrentTrust =n/v",
                //civ, otherPower.CivID, trustDelta, diplomat.Owner);
                if (diplomat.OwnerID == civId)
                    continue;

                var foreignPower = diplomat.GetForeignPower(civ);

                if (civId == otherPower.CivID)
                {
                    //var foreignPower = diplomat.GetForeignPower(civ);
                    GameLog.Core.Diplomacy.DebugFormat("BEFORE: civ = {0}, otherPower.CivID = {1}, trustDelta = {2}, diplomat.Owner = {3}, foreignPower.OwnerID = {4}, CurrentTrust = {5}",
                                    civ, otherPower.CivID, trustDelta, diplomat.Owner, foreignPower.OwnerID, foreignPower.DiplomacyData.Trust.CurrentValue);

                    if (foreignPower != null)
                        foreignPower.DiplomacyData.Trust.AdjustCurrent(trustDelta);
                    foreignPower.DiplomacyData.Trust.UpdateAndReset();

                    GameLog.Core.Diplomacy.DebugFormat("AFTER : civ = {0}, otherPower.CivID = {1}, trustDelta = {2}, diplomat.Owner = {3}, foreignPower.OwnerID = {4}, CurrentTrust = {5}",
                            civ, otherPower.CivID, trustDelta, diplomat.Owner, foreignPower.OwnerID, foreignPower.DiplomacyData.Trust.CurrentValue);
                }

                if (foreignPower != null)
                {
                    foreignPower.DiplomacyData.Trust.AdjustCurrent(trustDelta);
                    foreignPower.DiplomacyData.Trust.UpdateAndReset();
                    foreignPower.UpdateRegardAndTrustMeters();
                }
            }
        }



        // Regard makes crashes

        //public static void ApplyRegardChange([NotNull] ICivIdentity civ, [NotNull] ICivIdentity otherPower, int regardDelta)
        //{
        //    if (civ == null)
        //        throw new ArgumentNullException("civ");

        //    var civId = civ.CivID;
                       
        //    foreach (var diplomat in GameContext.Current.Diplomats)
        //    {
        //        var foreignPower = diplomat.GetForeignPower(civ);

        //        if (diplomat.OwnerID == civId)
        //            continue;

        //        if (foreignPower != null)
        //            foreignPower.DiplomacyData.Regard.AdjustCurrent(regardDelta);
        //        foreignPower.DiplomacyData.Regard.UpdateAndReset();
        //        foreignPower.UpdateRegardAndTrustMeters();
        //    }
        //}

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
            if (declaringCiv == null)
                throw new ArgumentNullException("declaringCiv");
            if (targetCiv == null)
                throw new ArgumentNullException("targetCiv");

            if (declaringCiv == targetCiv)
            {
                GameLog.Client.Diplomacy.ErrorFormat(
                    "Civilization {0} attempted to declare war on itself.",
                    declaringCiv.ShortName);

                return;
            }
          
            if (AreAtWar(declaringCiv, targetCiv))
            {
                GameLog.Client.Diplomacy.WarnFormat(
                    "Civilization {0} attempted to declare war on {1}, but they were already at war.",
                    declaringCiv.ShortName,
                    targetCiv.ShortName);

                return;                
            }

            var diplomat = Diplomat.Get(declaringCiv);
            var foreignPower = diplomat.GetForeignPower(targetCiv);

            var proposal = new Statement(declaringCiv, targetCiv, StatementType.WarDeclaration, tone);

            foreignPower.StatementSent = proposal;
            foreignPower.CounterpartyForeignPower.StatementReceived = proposal;
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
            if (traveller == null)
                throw new ArgumentNullException("traveller");
            if (sector == null)
                throw new ArgumentNullException("sector");

            var sectorOwner = sector.Owner;
            if (sectorOwner == null)
                sectorOwner = GameContext.Current.SectorClaims.GetOwner(sector.Location);

            if (sectorOwner == null || sectorOwner == traveller)
                return true;

            return !GameContext.Current.AgreementMatrix.IsAgreementActive(
                traveller,
                sectorOwner,
                ClauseType.TreatyNonAggression);
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
        /// Determines whether two particular <see cref="Civilization"/>s are at war
        /// </summary>
        public static bool AreAtWar(Civilization who, Civilization whoElse)
        {
            if (who == null)
                throw new ArgumentNullException("who");
            if (whoElse == null)
                throw new ArgumentNullException("whoElse");

            var diplomacyData = GameContext.Current.DiplomacyData[who, whoElse];

            return diplomacyData != null &&
                   diplomacyData.Status == ForeignPowerStatus.AtWar;
        }

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
                   agreementMatrix.IsAgreementActive(firstCiv, secondCiv, ClauseType.TreatyTradePact) ||
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

            GameLog.Core.Diplomacy.DebugFormat("firstManager.Civilization.Key = {0}, second = {1}", firstManager.Civilization.Key, secondManager.Civilization.Key);
            if (firstManager.Civilization.Key == "BORG")
            {
                foreignPower.DeclareWar();
                firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(firstCiv, secondCiv));
                secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(firstCiv, secondCiv));
                // playing 
                var soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgWeAreTheBorg.wav");  // ToDo - not working yet
                //soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgResistanceFutile.flac");
                //_soundPlayer.Play("Resources/SoundFX/TaskForceOrders/BorgWeAreTheBorg.mp3"); // at SitRep "Resistance is fut...."

                ApplyTrustChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Trust.CurrentValue * -1);
                //ApplyRegardChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Regard.CurrentValue * -1);


                //GameLog.Core.Diplomacy.DebugFormat("foreignPower = {3}, firstManager.Civilization.Key = {0}, second = {1}, TrustDelta {2}", 
                //    firstManager.Civilization.Key, secondManager.Civilization.Key, trustDelta, foreignPower.DiplomacyData);
            }

            if (secondManager.Civilization.Key == "BORG")
            {
                foreignPower.DeclareWar();
                firstManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                secondManager.SitRepEntries.Add(new WarDeclaredSitRepEntry(secondCiv, firstCiv));
                var soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgWeAreTheBorg.wav");  // ToDo - not working yet
                //soundPlayer = new SoundPlayer("Resources/SoundFX/TaskForceOrders/BorgResistanceFutile.flac");

                //ApplyTrustChange(secondCiv, firstCiv, ownPower.DiplomacyData.Trust.CurrentValue * -1);
                ApplyTrustChange(firstCiv, secondCiv, foreignPower.DiplomacyData.Trust.CurrentValue * -1);
                //ApplyRegardChange(secondCiv, firstCiv, ownPower.DiplomacyData.Regard.CurrentValue * -1);

                //GameLog.Core.Diplomacy.DebugFormat("secondManager.Civilization.Key = {0}, first = {1}, TrustDelta {2}", secondManager.Civilization.Key, firstManager.Civilization.Key, trustDelta);

            }

        }

        internal static void PerformFirstContacts(Civilization civilization, MapLocation location)
        {
            var otherCivs = new HashSet<int>();

            var colonies = from colony in GameContext.Current.Universe.FindAt<Colony>(location)
                           where colony.OwnerID != civilization.CivID
                           select colony;

            foreach (var item in colonies)
                otherCivs.Add(item.OwnerID);

            var fleets = from fleet in GameContext.Current.Universe.FindAt<Fleet>(location)
                         where fleet.OwnerID != civilization.CivID && !otherCivs.Contains(fleet.OwnerID)
                         let fleetView = FleetView.Create(civilization, fleet)
                         where fleetView.IsOwnerKnown
                         select fleet;

            foreach (var fleet in fleets)
                otherCivs.Add(fleet.OwnerID);

            foreach (var otherCiv in otherCivs)
                EnsureContact(civilization, GameContext.Current.Civilizations[otherCiv], location);
        }

        public static bool IsContactMade(Civilization source, Civilization target)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");
            
            if (source == target)
                return false;

            return GameContext.Current.DiplomacyData[source, target].IsContactMade();
        }

        public static bool IsContactMade(int sourceId, int targetId)
        {
            if (sourceId == targetId)
                return true;

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
    }
}
