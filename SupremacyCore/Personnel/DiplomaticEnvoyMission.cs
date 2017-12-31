using System;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Tech;
using Supremacy.Text;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public class DiplomaticEnvoyMission : Mission
    {
        private readonly GameObjectID _counterpartyId;

        public DiplomaticEnvoyMission([NotNull] Civilization owner, [NotNull] Civilization counterparty, MapLocation pointOfEmbarkation)
            : base(owner, pointOfEmbarkation)
        {
            if (counterparty == null)
                throw new ArgumentNullException("counterparty");
            GameLog.Client.GameData.DebugFormat("DioplomaticEnvoyMission.cs: owner, counterparty, pointOfEmbarkation: {0} - {1} - {2}", owner, counterparty, pointOfEmbarkation);
            _counterpartyId = counterparty.CivID;
        }

        public Civilization Counterparty
        {
            get { return GameContext.Current.Civilizations[_counterpartyId]; }
        }

        public Diplomat CounterpartyDiplomat
        {
            get
            {
                Diplomat diplomat;
                if (GameContext.Current.Diplomats.TryGetValue(_counterpartyId, out diplomat))
                {
                    //found this line to be cause of stack overflow loop GameLog.Client.GameData.DebugFormat("DioplomaticEnvoyMission.cs: CounterpartyDiplomat, _counterpartyId: {0} - {1}", CounterpartyDiplomat, _counterpartyId);
                    return diplomat;
                }
                return null;
            }
        }

        public Diplomat OwnerDiplomat
        {
            get { return GameContext.Current.Diplomats[Owner]; }
        }

        public override string DescriptionText
        {
            get
            {
                return string.Format(
                    LocalizedTextDatabase.Instance.Groups[typeof(DiplomaticEnvoyMission)].Entries["DescriptionText"].LocalText,
                    Counterparty.ShortName);
            }
        }

        protected override void BeginCore()
        {
            TransitionToPhase(new OutboundPhase(this));
        }

        protected override bool CanAssignCore(Agent agent)
        {
            return !AssignedAgents.Any();
        }

        protected override void AssignCore(Agent agent)
        {
            var myViewOfThem = OwnerDiplomat.GetForeignPower(Counterparty);
            var counterpartyDiplomat = CounterpartyDiplomat;
            var theirViewOfMe = counterpartyDiplomat != null ? counterpartyDiplomat.GetForeignPower(Owner) : null;

            myViewOfThem.AssignedEnvoy = agent;
            
            if (theirViewOfMe != null)
                theirViewOfMe.CounterpartyEnvoy = agent;
        }

        protected override void UnassignCore(Agent agent)
        {
            var myViewOfThem = OwnerDiplomat.GetForeignPower(Counterparty);
            var counterpartyDiplomat = CounterpartyDiplomat;
            var theirViewOfMe = counterpartyDiplomat != null ? counterpartyDiplomat.GetForeignPower(Owner) : null;

            if (myViewOfThem.AssignedEnvoy == agent)
                myViewOfThem.AssignedEnvoy = null;

            if (theirViewOfMe != null && theirViewOfMe.CounterpartyEnvoy == agent)
                theirViewOfMe.CounterpartyEnvoy = null;
        }

        protected override bool CancelCore(bool force)
        {
            var totalDistance = MapLocation.GetDistance(
                OwnerDiplomat.SeatOfGovernment.Location,
                CurrentLocation ?? CounterpartyDiplomat.SeatOfGovernment.Location);

            if (totalDistance > 0)
                return TransitionToPhase(new InboundPhase(this), force);

            return TransitionToPhase(new MissionPhase.Completed(this, false), force);
        }

        #region OutboundPhase Class

        [Serializable]
        public sealed class OutboundPhase : MissionPhase
        {
            private int _turnsUntilArrival;

            public OutboundPhase([NotNull] DiplomaticEnvoyMission mission)
                : base(mission) {}

            public int TurnsUntilArrival
            {
                get { return _turnsUntilArrival; }
            }

            public new DiplomaticEnvoyMission Mission
            {
                get { return (DiplomaticEnvoyMission)base.Mission; }
            }

            #region Overrides of MissionPhase

            public override string StatusText
            {
                get
                {
                    return string.Format(
                        LocalizedTextDatabase.Instance.Groups[typeof(DiplomaticEnvoyMission)].Entries["OutboundPhaseStatusText"].LocalText,
                        Mission.Counterparty.HomeSystemName);
                }
            }

            #endregion

            protected internal override void OnTransitionedTo(MissionPhase lastPhase)
            {
                var distanceUntilArrival = MapLocation.GetDistance(
                    Mission.OwnerDiplomat.SeatOfGovernment.Location,
                    Mission.CounterpartyDiplomat.SeatOfGovernment.Location);

                if (distanceUntilArrival <= 0)
                {
                    _turnsUntilArrival = 0;
                    return;
                }

                var speed = TechTreeHelper.GetDesignsForCurrentTechLevels(Mission.Owner)
                    .OfType<ShipDesign>()
                    .Select(d => d.Speed)
                    .DefaultIfEmpty(3)
                    .Max();

                var turns = distanceUntilArrival / speed;

                if (distanceUntilArrival % speed != 0)
                    ++turns;

                _turnsUntilArrival = turns;
            }

            public override bool CanTransitionTo(MissionPhase proposedPhase)
            {
                if (Mission.IsCancelled)
                    return (proposedPhase is InboundPhase);

                return _turnsUntilArrival == 0;
            }

            public override bool IsMissionCancellationAllowed
            {
                get { return false; }
            }

            public override int? EstimatedTurnsRemaining
            {
                get { return _turnsUntilArrival; }
            }

            protected internal override void OnTurnPhaseFinished(TurnPhase phase)
            {
                if (phase != TurnPhase.Combat)
                    return;

                if (_turnsUntilArrival > 0)
                    --_turnsUntilArrival;
                else
                    Mission.TransitionToPhase(new StationedPhase(Mission));
            }
        }

        #endregion

        #region InboundPhase Class

        [Serializable]
        public sealed class InboundPhase : MissionPhase
        {
            private int _turnsUntilArrival;

            public InboundPhase([NotNull] DiplomaticEnvoyMission mission)
                : base(mission) {}

            public new DiplomaticEnvoyMission Mission
            {
                get { return (DiplomaticEnvoyMission)base.Mission; }
            }

            #region Overrides of MissionPhase

            public override string StatusText
            {
                get
                {
                    return string.Format(
                        LocalizedTextDatabase.Instance.Groups[typeof(DiplomaticEnvoyMission)].Entries["InboundPhaseStatusText"].LocalText,
                        Mission.OwnerDiplomat.SeatOfGovernment.Name);
                }
            }

            #endregion

            protected internal override void OnTransitionedTo(MissionPhase lastPhase)
            {
                var outboundPhase = lastPhase as OutboundPhase;

                var totalDistance = 0;

                CivilizationManager civManager = GameContext.Current.CivilizationManagers[Mission.Counterparty.Key];
                if( civManager != null)
                {
                    totalDistance = MapLocation.GetDistance(
                        Mission.OwnerDiplomat.SeatOfGovernment.Location,
                        civManager.SeatOfGovernment.Location);
                }
                
                if (totalDistance <= 0)
                {
                    // This should never happen.
                    _turnsUntilArrival = 0;
                    return;
                }

                var speed = TechTreeHelper.GetDesignsForCurrentTechLevels(Mission.Owner)
                    .OfType<ShipDesign>()
                    .Select(d => d.Speed)
                    .DefaultIfEmpty(3)
                    .Max();

                var turns = totalDistance / speed;

                if (totalDistance % speed != 0)
                    ++turns;

                if (outboundPhase != null)
                    turns = Math.Max(0, turns - outboundPhase.TurnsUntilArrival);

                _turnsUntilArrival = turns;
            }

            public override bool CanTransitionTo(MissionPhase proposedPhase)
            {
                if (proposedPhase is StationedPhase)
                    return TurnStarted == GameContext.Current.TurnNumber;
                return _turnsUntilArrival == 0;
            }

            public override bool IsMissionCancellationAllowed
            {
                get { return false; }
            }

            public override int? EstimatedTurnsRemaining
            {
                get { return _turnsUntilArrival; }
            }

            protected internal override void OnTurnPhaseFinished(TurnPhase phase)
            {
                if (phase != TurnPhase.Combat)
                    return;

                if (_turnsUntilArrival > 0)
                    --_turnsUntilArrival;
                else
                    Mission.Complete(!Mission.IsCancelled);
            }
        }

        #endregion

        #region StationedPhase Class

        [Serializable]
        public sealed class StationedPhase : MissionPhase
        {
            public StationedPhase([NotNull] DiplomaticEnvoyMission mission)
                : base(mission) {}

            public new DiplomaticEnvoyMission Mission
            {
                get { return (DiplomaticEnvoyMission)base.Mission; }
            }

            #region Overrides of MissionPhase

            public override string StatusText
            {
                get
                {
                    return string.Format(
                        LocalizedTextDatabase.Instance.Groups[typeof(DiplomaticEnvoyMission)].Entries["StationedPhaseStatusText"].LocalText,
                        Mission.Counterparty.HomeSystemName);
                }
            }

            #endregion

            public override bool CanTransitionTo(MissionPhase proposedPhase)
            {
                return (proposedPhase is InboundPhase);
            }

            public override bool IsMissionCancellationAllowed
            {
                get { return true; }
            }

            public override int? EstimatedTurnsRemaining
            {
                get { return null; }
            }

            protected internal override void OnTransitionedTo(MissionPhase lastPhase)
            {
                base.OnTransitionedTo(lastPhase);
                Mission.CurrentLocation = Mission.CounterpartyDiplomat.SeatOfGovernment.Location;
            }

            protected internal override void OnTurnPhaseFinished(TurnPhase phase)
            {
                if (phase == TurnPhase.Diplomacy)
                {
                    /*
                     * Check to see if we went to war with our counterparty.  If we did, we need to
                     * abort the mission and get back home.
                     */
                    if (DiplomacyHelper.AreAtWar(Mission.Owner, Mission.Counterparty))
                        Mission.Cancel();
                }
            }
        }

        #endregion
    }
}