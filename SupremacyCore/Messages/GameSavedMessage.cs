using Supremacy.Game;

namespace Supremacy.Messages
{
    public class GameSavedMessage
    {
        public GameSavedMessage(SavedGameHeader savedGameHeader = null)
        {
            SavedGameHeader = savedGameHeader;
        }

        public SavedGameHeader SavedGameHeader { get; }
    }
}