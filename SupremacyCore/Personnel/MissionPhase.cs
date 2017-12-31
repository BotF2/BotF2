using System;

using Supremacy.Annotations;
using Supremacy.Game;

namespace Supremacy.Personnel
{
    [Serializable]
    public abstract class MissionPhase : IEquatable<MissionPhase>
    {
        private readonly Mission _mission;
        private readonly TurnNumber _turnStarted;

        protected MissionPhase([NotNull] Mission mission)
        {
            if (mission == null)
                throw new ArgumentNullException("mission");

            _mission = mission;
            _turnStarted = GameContext.Current.TurnNumber;
        }

        [NotNull]
        public Mission Mission
        {
            get { return _mission; }
        }

        protected TurnNumber TurnStarted
        {
            get { return _turnStarted; }
        }

        public virtual bool IsMissionCancellationAllowed
        {
            get { return true; }
        }

        public virtual bool AreAssignmentsChangesAllowed
        {
            get { return false; }
        }

        public virtual int? EstimatedTurnsRemaining
        {
            get { return null; }
        }

        [NotNull]
        public abstract string StatusText { get; }

        [CanBeNull]
        public virtual string DescriptionText { get { return null; } }

        public virtual bool Equals(MissionPhase otherPhase)
        {
            return ReferenceEquals(otherPhase, this);
        }

        protected internal virtual void OnTransitionedTo([CanBeNull] MissionPhase lastPhase) {}

        public virtual bool CanTransitionTo(MissionPhase proposedPhase)
        {
            return true;
        }

        #region Planning Class

        [Serializable]
        public sealed class Planning : MissionPhase
        {
            public Planning([NotNull] Mission mission)
                : base(mission) { }

            public override bool CanTransitionTo(MissionPhase proposedPhase)
            {
                return true;
            }

            public override bool AreAssignmentsChangesAllowed
            {
                get { return true; }
            }

            public override string StatusText
            {
                get { return "Preparing for Departure"; }
            }

            public override bool Equals(MissionPhase otherPhase)
            {
                if (ReferenceEquals(otherPhase, null))
                    return false;

                if (!Equals(otherPhase._mission, _mission))
                    return false;

                return (otherPhase is Planning);
            }
        }

        #endregion

        #region Completed Class

        [Serializable]
        public class Completed : MissionPhase
        {
            private readonly bool _completedSuccessfully;

            public Completed([NotNull] Mission mission, bool completedSuccessfully)
                : base(mission)
            {
                _completedSuccessfully = completedSuccessfully;
            }

            public bool CompletedSuccessfully
            {
                get { return _completedSuccessfully; }
            }

            public sealed override bool CanTransitionTo(MissionPhase proposedPhase)
            {
                return false;
            }

            public override bool IsMissionCancellationAllowed
            {
                get { return false; }
            }

            public override string StatusText
            {
                get { return "Completed"; }
            }

            public override bool Equals(MissionPhase otherPhase)
            {
                if (ReferenceEquals(otherPhase, null))
                    return false;

                if (!Equals(otherPhase._mission, _mission))
                    return false;

                var otherCompletedPhase = otherPhase as Completed;

                return otherCompletedPhase != null &&
                       otherCompletedPhase._completedSuccessfully == _completedSuccessfully;
            }
        }

        #endregion

        protected internal virtual void OnTurnStarted() {}
        protected internal virtual void OnTurnPhaseStarted(TurnPhase phase) {}
        protected internal virtual void OnTurnPhaseFinished(TurnPhase phase) {}
        protected internal virtual void OnTurnFinished() {}
    }
}