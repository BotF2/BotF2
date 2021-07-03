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
            _player = player ?? throw new ArgumentNullException("player");
        }

        public IPlayer Player => _player;
    }
}