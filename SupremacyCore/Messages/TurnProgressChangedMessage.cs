using Supremacy.Game;

namespace Supremacy.Messages
{
    public class TurnProgressChangedMessage
    {
        private readonly TurnPhase _turnPhase;

        public TurnProgressChangedMessage(TurnPhase turnPhase)
        {
            _turnPhase = turnPhase;
        }

        public TurnPhase TurnPhase
        {
            get { return _turnPhase; }
        }
    }
}