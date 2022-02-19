using Supremacy.Game;

namespace Supremacy.Messages
{
    public class TurnProgressChangedMessage
    {
        public TurnProgressChangedMessage(TurnPhase turnPhase)
        {
            TurnPhase = turnPhase;
        }

        public TurnPhase TurnPhase { get; }
    }
}