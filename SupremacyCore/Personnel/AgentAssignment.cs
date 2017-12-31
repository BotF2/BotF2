using System;
using System.ComponentModel;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public abstract class AgentAssignment : IGameTurnListener
    {
        private readonly GameObjectID _ownerId;
        private readonly CollectionBase<ObjectOwnerPair> _assignedAgents;
        private readonly DelegatingIndexedCollection<Agent, IIndexedCollection<ObjectOwnerPair>> _assignedAgentsView;

        private AgentAssignmentState _state;
        [NonSerialized] private bool _isCancelling;

        public event EventHandler<EventArgs> Cancelled;
        public event EventHandler<EventArgs> Completed;
        public event EventHandler<CancelEventArgs> Cancelling;
        public event EventHandler<AgentEventArgs> AgentAssigned;
        public event EventHandler<AgentEventArgs> AgentUnassigned;

        protected AgentAssignment([NotNull] Civilization owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _ownerId = owner.CivID;

            _assignedAgents = new CollectionBase<ObjectOwnerPair>();

            _assignedAgentsView = new DelegatingIndexedCollection<Agent, IIndexedCollection<ObjectOwnerPair>>(
                _assignedAgents,
                pairs => from pair in pairs
                         let civManager = GameContext.Current.CivilizationManagers[pair.OwnerID]
                         select civManager.AgentPool.CurrentAgents[pair.ObjectID],
                collection => collection.Count,
                (pairs, i) => GameContext.Current.CivilizationManagers[pairs[i].OwnerID].AgentPool.CurrentAgents[pairs[i].ObjectID]);

            _state = AgentAssignmentState.Planning;
        }

        public AgentAssignmentState State
        {
            get { return _state; }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public abstract string StatusText { get; }
        public abstract string DescriptionText { get; }

        public bool HasAssignedAgents
        {
            get { return _assignedAgents.Count != 0; }
        }

        public bool IsCancelling
        {
            get { return _isCancelling; }
        }

        public bool IsCompleted
        {
            get { return (State == AgentAssignmentState.Completed); }
        }

        public bool IsProgressKnown
        {
            get { return ProgressInternal.HasValue; }
        }

        public bool CanCancel
        {
            get
            {
                if (State == AgentAssignmentState.Planning)
                    return true;
                return CanCancelCore();
            }
        }

        public Percentage Progress
        {
            get { return ProgressInternal ?? Percentage.MinValue; }
        }

        protected virtual Percentage? ProgressInternal
        {
            get { return null; }
        }

        public IIndexedCollection<Agent> AssignedAgents
        {
            get { return _assignedAgentsView; }
        }

        public bool CanAssign([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            if (State != AgentAssignmentState.Planning)
                return false;

            if (agent.Owner != Owner)
                return false;

            CivilizationManager civManager;

            if (!GameContext.Current.CivilizationManagers.TryGetValue(_ownerId, out civManager))
                return false;

            if (!civManager.AgentPool.CurrentAgents.Contains(agent))
                return false;

            return CanAssignCore(agent);
        }

        public bool Assign([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            if (!CanAssign(agent))
                return false;

            AssignCore(agent);

            agent.Assignment = this;
            _assignedAgents.Add(new ObjectOwnerPair(agent.ObjectID, agent.OwnerID));

            AgentAssigned.Raise(this, new AgentEventArgs(agent));

            return true;
        }

        public bool Unassign([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            if (!AssignedAgents.Contains(agent))
                return false;

            if (State != AgentAssignmentState.Planning && !CanUnassignCore(agent))
                return false;

            UnassignCore(agent);

            agent.Assignment = null;
            _assignedAgents.RemoveAt(0);

            AgentUnassigned.Raise(this, new AgentEventArgs(agent));

            return true;
        }

        protected void UnassignAllAgents()
        {
            while (AssignedAgents.Count > 0)
                Unassign(AssignedAgents[0]);
        }

        public bool Cancel()
        {
            if (!CanCancel)
                return false;

            _isCancelling = true;

            var cancellingEventArgs = new CancelEventArgs(false);

            Cancelling.Raise(this, cancellingEventArgs);

            if (cancellingEventArgs.Cancel)
                return false;

            try
            {
                CancelCore();

                if (HasAssignedAgents)
                    UnassignAllAgents();
            }
            finally
            {
                _isCancelling = false;
                _state = AgentAssignmentState.Cancelled;
            }

            Cancelled.Raise(this);

            return true;
        }

        protected void Complete()
        {
            if (_state == AgentAssignmentState.Cancelled || _state == AgentAssignmentState.Completed)
                return;

            try
            {
                CompleteCore();
            }
            finally
            {
                _state = AgentAssignmentState.Completed;
            }

            Completed.Raise(this);
        }

        protected virtual void CompleteCore()
        {
            UnassignAllAgents();
        }

        protected virtual bool CanCancelCore() { return true; }
        protected virtual void CancelCore() {}

        protected abstract bool CanAssignCore([NotNull] Agent agent);
        protected abstract void AssignCore([NotNull] Agent agent);
        protected abstract void UnassignCore([NotNull] Agent agent);
        protected abstract bool CanUnassignCore([NotNull] Agent agent);

        protected virtual void OnTurnStarted() {}
        protected virtual void OnTurnPhaseStarted(TurnPhase phase) {}
        protected virtual void OnTurnPhaseFinished(TurnPhase phase) {}
        protected virtual void OnTurnFinished() {}

        #region Implementation of IGameTurnListener

        void IGameTurnListener.OnTurnStarted(GameContext game)
        {
            if (_state == AgentAssignmentState.Planning)
                _state = AgentAssignmentState.InProgress;

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
    }
}