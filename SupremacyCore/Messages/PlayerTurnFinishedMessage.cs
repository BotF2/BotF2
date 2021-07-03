using System;

using Supremacy.Annotations;
using Supremacy.Game;

namespace Supremacy.Messages
{
    public class PlayerTurnFinishedMessage
    {
        public PlayerTurnFinishedMessage([NotNull] IPlayer player)
        {
            Player = player ?? throw new ArgumentNullException("player");
        }

        public IPlayer Player { get; }
    }
}