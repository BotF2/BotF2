// IGameTurnListener.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

namespace Supremacy.Game
{
    public interface IGameTurnListener
    {
        void OnTurnStarted(GameContext game);
        void OnTurnPhaseStarted(GameContext game, TurnPhase phase);
        void OnTurnPhaseFinished(GameContext game, TurnPhase phase);
        void OnTurnFinished(GameContext game);
    }
}