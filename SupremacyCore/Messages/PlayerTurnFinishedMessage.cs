using System;

using Supremacy.Annotations;
using Supremacy.Game;

namespace Supremacy.Messages
{
    public class PlayerTurnFinishedMessage
    {
        private readonly IPlayer _player;

        public PlayerTurnFinishedMessage([NotNull] IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException("player");
            _player = player;
        }

        public IPlayer Player
        {
            get { return _player; }
        }
    }
}