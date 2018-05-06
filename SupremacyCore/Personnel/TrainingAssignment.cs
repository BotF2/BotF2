using System;

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Text;

namespace Supremacy.Personnel
{
    [Serializable]
    public class TrainingAssignment : AgentAssignment
    {
        private readonly AgentCareer _career;
        private readonly int _startTurn;
        private readonly int _endTurn;

        public TrainingAssignment(Civilization owner, AgentCareer career)
            : base(owner)
        {
            if (career == AgentCareer.None)
                throw new ArgumentException("'None' is not a valid value.", "career");

            _career = career;
            _startTurn = GameContext.Current.TurnNumber;
            _endTurn = _startTurn + PersonnelConstants.Instance.TrainingDuration;
        }

        #region Overrides of AgentAssignment

        protected override Types.Percentage? ProgressInternal
        {
            get
            {
                var turnsRemaining = (float)_endTurn - GameContext.Current.TurnNumber;
                var totalTurns = (float)_endTurn - _startTurn;

                return (turnsRemaining / totalTurns);
            }
        }
        public override string StatusText
        {
            get { return LocalizedTextDatabase.Instance.Groups[typeof(TrainingAssignment)].Entries["StatusText"].LocalText; }
        }

        public override string DescriptionText
        {
            get
            {
                LocalizedString agentName = null;
                
                //var assignedAgent = this.AssignedAgent;
                //if (assignedAgent != null)
                //    agentName = assignedAgent.Profile.DisplayName;

                return string.Format(
                    LocalizedTextDatabase.Instance.Groups[typeof(TrainingAssignment)].Entries["DescriptionText"].LocalText,
                    agentName,
                    LocalizedTextDatabase.Instance.Groups[typeof(AgentCareer)].Entries[_career.ToString()]);
            }
        }

        protected override bool CanAssignCore(Agent agent)
        {
            return !HasAssignedAgents;
        }

        protected override bool CanUnassignCore(Agent agent)
        {
            return HasAssignedAgents &&
                   Equals(AssignedAgents[0], agent);
        }

        protected override bool CanCancelCore()
        {
            return true;
        }

        protected override void CancelCore() {}

        protected override void AssignCore(Agent agent) {}

        protected override void UnassignCore(Agent agent)
        {
            if (!IsCompleted)
                return;

            //this.AssignedAgent.Career = _career;
        }

        #endregion
    }
}