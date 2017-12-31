using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Text;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public abstract class Mission : IGameTurnListener, INotifyPropertyChanged
    {
        private readonly GameObjectID _ownerId;
        private readonly MapLocation _pointOfEmbarkation;
        private readonly CollectionBase<ObjectOwnerPair> _assignedAgents;
        private readonly DelegatingIndexedCollection<Agent, IIndexedCollection<ObjectOwnerPair>> _assignedAgentsView;

        private TurnNumber _cancellationTurn;
        private MissionPhase _currentPhase;
        [NonSerialized] private MissionPhase _previousPhase;
        private MapLocation? _currentLocation;

        public event EventHandler<MissionEventArgs> Completed;
        public event EventHandler<MissionEventArgs> Cancelled;
        public event EventHandler<MissionEventArgs> CancellationRescinded;
        public event EventHandler<AgentEventArgs> AgentAssigned;
        public event EventHandler<AgentEventArgs> AgentUnassigned;
        public event EventHandler<MissionPhaseChangedEventArgs> PhaseChanged;

        public static Mission CreateDefaultMission([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            return new NullMission(
                agent.Owner,
                GameContext.Current.CivilizationManagers[agent.OwnerID].SeatOfGovernment.Location);
        }

        protected Mission([NotNull] Civilization owner, MapLocation pointOfEmbarkation)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _ownerId = owner.CivID;
            _pointOfEmbarkation = pointOfEmbarkation;
            _currentLocation = _pointOfEmbarkation;

            _assignedAgents = new CollectionBase<ObjectOwnerPair>();

            _assignedAgentsView = new DelegatingIndexedCollection<Agent, IIndexedCollection<ObjectOwnerPair>>(
                _assignedAgents,
                pairs => from pair in pairs
                         let civManager = GameContext.Current.CivilizationManagers[pair.OwnerID]
                         select civManager.AgentPool.CurrentAgents[pair.ObjectID],
                collection => collection.Count,
                (pairs, i) => GameContext.Current.CivilizationManagers[pairs[i].OwnerID].AgentPool.CurrentAgents[pairs[i].ObjectID]);

            _currentPhase = new MissionPhase.Planning(this);
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public MapLocation PointOfEmbarkation
        {
            get { return _pointOfEmbarkation; }
        }

        public MapLocation? CurrentLocation
        {
            get { return _currentLocation; }
            protected internal set { _currentLocation = value; }
        }

        public IIndexedCollection<Agent> AssignedAgents
        {
            get { return _assignedAgentsView; }
        }

        public bool IsCancelled
        {
            get { return !_cancellationTurn.IsUndefined; }
        }

        public bool IsInPlanningStage
        {
            get { return _currentPhase is MissionPhase.Planning; }
        }

        public bool IsCompleted
        {
            get { return _currentPhase is MissionPhase.Completed; }
        }

        public bool CompletedSuccessfully
        {
            get { return IsCompleted && _cancellationTurn.IsUndefined; }
        }

        [NotNull]
        public MissionPhase CurrentPhase
        {
            get { return _currentPhase; }
        }

        public virtual string DescriptionText
        {
            get { return CurrentPhase.DescriptionText ?? GetType().Name; }
        }

        public virtual string StatusText
        {
            get
            {
                var statusText = CurrentPhase.StatusText;
                if (statusText == null)
                    throw new InvalidOperationException("MissionPhsae.StatusText cannot be null.");
                return statusText;
            }
        }

        public bool CanCancel
        {
            get { return !IsCancelled && CanCancelCore(); }
        }

        public bool CanUndoCancel
        {
            get { return _cancellationTurn == GameContext.Current.TurnNumber; }
        }

        public bool Cancel(bool force = false)
        {
            if (!force && !CanCancel)
                return false;

            if (CurrentPhase is MissionPhase.Planning)
                return TransitionToPhase(new MissionPhase.Completed(this, false), force);

            _cancellationTurn = GameContext.Current.TurnNumber;

            if (!CancelCore(force))
            {
                _cancellationTurn = TurnNumber.Undefined;
                return false;
            }

            OnCancelled();
            OnPropertyChanged();

            return true;
        }

        public bool UndoCancel()
        {
            if (!IsCancelled || _cancellationTurn != GameContext.Current.TurnNumber)
                return false;

            var cancelledPhase = _currentPhase;

            _cancellationTurn = TurnNumber.Undefined;
            _currentPhase = _previousPhase;
            _previousPhase = null;

            OnPhaseChanged(cancelledPhase, _currentPhase);
            OnCancellationRescinded();
            OnPropertyChanged();

            return true;
        }

        protected virtual void OnCancellationRescinded()
        {
            var handler = CancellationRescinded;
            if (handler != null)
                handler.Raise(this, new MissionEventArgs(this));
        }

        protected virtual void OnCancelled()
        {
            var handler = Cancelled;
            if (handler != null)
                handler(this, new MissionEventArgs(this));
        }

        protected virtual bool CanCancelCore()
        {
            return _currentPhase.IsMissionCancellationAllowed;
        }

        protected virtual bool CancelCore(bool force)
        {
            return TransitionToPhase(new MissionPhase.Completed(this, false), force);
        }

        protected virtual bool Complete(bool completedSuccessfully)
        {
            return TransitionToPhase(new MissionPhase.Completed(this, completedSuccessfully), true);
        }

        public virtual bool CanTransitionToPhase([NotNull] MissionPhase phase, bool force = false)
        {
            if (phase == null)
                throw new ArgumentNullException("phase");

            if (phase.Mission != this)
                return false;

            if (force)
                return true;

            return CanTransitionToPhaseCore(phase);
        }

        protected virtual bool CanTransitionToPhaseCore([NotNull] MissionPhase phase)
        {
            return _currentPhase.CanTransitionTo(phase);
        }

        protected internal virtual bool TransitionToPhase([NotNull] MissionPhase phase, bool force = false)
        {
            if (!CanTransitionToPhase(phase, force))
                return false;

            var lastPhase = _currentPhase;

            _currentPhase = phase;

            phase.OnTransitionedTo(lastPhase);

            _previousPhase = lastPhase;

            OnPhaseChanged(lastPhase, _currentPhase);

            if (_currentPhase is MissionPhase.Completed)
                OnCompleted();

            return true;
        }

        protected virtual void OnCompleted()
        {
            var handler = Completed;
            if (handler != null)
                handler(this, new MissionEventArgs(this));

            UnassignAllAgents();
        }

        protected virtual void OnPhaseChanged([NotNull] MissionPhase oldPhase, [NotNull] MissionPhase newPhase)
        {
            var phaseChanged = PhaseChanged;
            if (phaseChanged != null)
                phaseChanged(this, new MissionPhaseChangedEventArgs(oldPhase, newPhase));

            OnPropertyChanged();
        }

        public bool CanAssign([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            if (!CurrentPhase.AreAssignmentsChangesAllowed)
                return false;

            if (agent.Owner != Owner)
                return false;

            //CivilizationManager civManager;

            //if (!GameContext.Current.CivilizationManagers.TryGetValue(_ownerId, out civManager))
            //    return false;

            //if (!civManager.AgentPool.CurrentAgents.Contains(agent))
            //    return false;

            return CanAssignCore(agent);
        }

        public bool Assign([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            if (!CanAssign(agent))
                return false;

            AssignCore(agent);

            agent.Mission = this;
            
            _assignedAgents.Add(new ObjectOwnerPair(agent.ObjectID, agent.OwnerID));

            OnAgentAssigned(agent);

            return true;
        }

        protected virtual void OnAgentAssigned(Agent agent)
        {
            var handler = AgentAssigned;
            if (handler != null)
                handler(this, new AgentEventArgs(agent));
        }

        public bool Unassign([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            if (!AssignedAgents.Contains(agent))
                return false;

            if (!IsInPlanningStage && !IsCancelled && !IsCompleted)
                return false;

            UnassignCore(agent);

            agent.Mission = new NullMission(Owner, PointOfEmbarkation);

            _assignedAgents.RemoveAt(0);

            OnAgentUnassigned(agent);

            return true;
        }

        protected virtual void OnAgentUnassigned(Agent agent)
        {
            var handler = AgentUnassigned;
            if (handler != null)
                handler.Raise(this, new AgentEventArgs(agent));
        }

        public void Begin()
        {
            if (!IsInPlanningStage)
                throw new InvalidOperationException("Mission has already commenced.");

            BeginCore();
        }

        private void UnassignAllAgents()
        {
            while (AssignedAgents.Count > 0 && Unassign(AssignedAgents[0]))
                continue;
        }

        protected abstract void BeginCore();
        protected abstract bool CanAssignCore([NotNull] Agent agent);
        protected abstract void AssignCore([NotNull] Agent agent);
        protected abstract void UnassignCore([NotNull] Agent agent);

        protected virtual void OnTurnStarted()
        {
            _currentPhase.OnTurnStarted();
        }

        protected virtual void OnTurnPhaseStarted(TurnPhase phase)
        {
            _currentPhase.OnTurnPhaseStarted(phase);
        }

        protected virtual void OnTurnPhaseFinished(TurnPhase phase)
        {
            _currentPhase.OnTurnPhaseFinished(phase);
        }

        protected virtual void OnTurnFinished()
        {
            _currentPhase.OnTurnFinished();
        }

        #region Implementation of IGameTurnListener

        void IGameTurnListener.OnTurnStarted(GameContext game)
        {
            if (IsInPlanningStage)
                Begin();

            GameContext.PushThreadContext(game);
            try { OnTurnStarted(); }
            finally { GameContext.PopThreadContext(); }
        }

        void IGameTurnListener.OnTurnPhaseStarted(GameContext game, TurnPhase phase)
        {
            GameContext.PushThreadContext(game);
            try { OnTurnPhaseStarted(phase); }
            finally { GameContext.PopThreadContext(); }
        }

        void IGameTurnListener.OnTurnPhaseFinished(GameContext game, TurnPhase phase)
        {
            GameContext.PushThreadContext(game);
            try { OnTurnPhaseFinished(phase); }
            finally { GameContext.PopThreadContext(); }
        }

        void IGameTurnListener.OnTurnFinished(GameContext game)
        {
            GameContext.PushThreadContext(game);
            try { OnTurnFinished(); }
            finally { GameContext.PopThreadContext(); }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [NonSerialized] private PropertyChangedEventHandler _propertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Combine(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
            remove
            {
                while (true)
                {
                    var oldHandler = _propertyChanged;
                    var newHandler = (PropertyChangedEventHandler)Delegate.Remove(oldHandler, value);

                    if (Interlocked.CompareExchange(ref _propertyChanged, newHandler, oldHandler) == oldHandler)
                        return;
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            _propertyChanged.Raise(this, propertyName);
        }

        #endregion
    }

    [Serializable]
    public sealed class NullMission : Mission
    {
        public NullMission([NotNull] Civilization owner, MapLocation pointOfEmbarkation)
            : base(owner, pointOfEmbarkation)
        {
            TransitionToPhase(new NullMissionPhase(this));
        }

        #region Overrides of Mission

        protected override void BeginCore() {}

        protected override bool CanAssignCore(Agent agent)
        {
            return (AssignedAgents.Count == 0);
        }

        protected override void AssignCore(Agent agent) {}

        protected override void UnassignCore(Agent agent) {}

        #endregion

        [Serializable]
        private sealed class NullMissionPhase : MissionPhase
        {
            public NullMissionPhase([NotNull] NullMission mission)
                : base(mission) {}

            #region Overrides of MissionPhase

            public override string DescriptionText
            {
                get { return LocalizedTextDatabase.Instance.Groups[typeof(NullMission)].Entries["DescriptionText"].LocalText; }
            }

            public override bool AreAssignmentsChangesAllowed
            {
                get { return true; }
            }

            public override string StatusText
            {
                get { 
                    var formatString = LocalizedTextDatabase.Instance.Groups[typeof(NullMission)].Entries["StatusText"].LocalText;

                    var sector = (Sector)null;
                    var location = Mission.CurrentLocation;

                    if (location.HasValue)
                        sector = GameContext.Current.Universe.Map[location.Value];

                    return string.Format(formatString, sector);
                }
            }

            #endregion
        }
    }
}