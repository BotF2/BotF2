using System;

using Supremacy.Annotations;
using Supremacy.Diplomacy;
using Supremacy.Entities;

namespace Supremacy.Personnel
{
    public static class AgentHelper
    {
        public static void TransferToSeatOfGovernment([NotNull] Agent agent)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");

            TransferToSeatOfGovernment(agent, agent.Owner);
        }

        public static bool TransferToSeatOfGovernment([NotNull] Agent agent, [NotNull] Civilization targetGovernment)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");
            if (targetGovernment == null)
                throw new ArgumentNullException("targetGovernment");

            var seatOfGovernment = DiplomacyHelper.GetSeatOfGovernment(targetGovernment);
            if (seatOfGovernment == null)
                return false;
            
            agent.CurrentLocation = seatOfGovernment.Location;

            return true;
        }
    }
}