using Supremacy.Game;

namespace Supremacy.Messages
{
    public class GameSavedMessage
    {
        private readonly SavedGameHeader _savedGameHeader;

        public GameSavedMessage(SavedGameHeader savedGameHeader = null)
        {
            _savedGameHeader = savedGameHeader;
        }

        public SavedGameHeader SavedGameHeader
        {
            get { return _savedGameHeader; }
        }
    }
}