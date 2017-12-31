using System;

using Supremacy.Annotations;

namespace Supremacy.Personnel
{
    [Serializable]
    public class AgentEventArgs : EventArgs
    {
        private readonly Agent _agent;

        public AgentEventArgs([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            _agent = agent;
        }

        public Agent Agent
        {
            get { return _agent; }
        }
    }
}