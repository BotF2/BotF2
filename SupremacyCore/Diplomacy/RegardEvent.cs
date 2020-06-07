// RegardEvent.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class RegardEvent
    {
        public int Turn { get; protected set; }
        public int Duration { get; protected set; }
        public int Regard { get; set; }
        public RegardEventType Type { get; protected set; }

        public RegardEvent(int duration, RegardEventType type)
            : this(duration, type, 0) { }

        public RegardEvent(int duration, RegardEventType type, int regard)
            : this(GameContext.Current.TurnNumber, duration, type, regard)
        {
            Turn = GameContext.Current.TurnNumber;
            Duration = duration;
            Type = type;
            Regard = regard;
        }

        public RegardEvent(int turn, int duration, RegardEventType type, int regard)
        {
            Turn = turn;
            Duration = duration;
            Type = type;
            Regard = regard;
        }
    }
}