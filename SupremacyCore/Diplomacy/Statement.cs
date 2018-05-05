// Statement.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class Statement : IDiplomaticExchange
    {
        public Statement(Civilization sender, Civilization recipient, StatementType statementType, Tone tone, object parameter = null, int turnNumber = 0)
        {
            TurnSent = turnNumber == 0 ? GameContext.Current.TurnNumber : turnNumber;
            Sender = sender;
            Recipient = recipient;
            StatementType = statementType;
            Tone = tone;
            Parameter = parameter;
        }

        public int TurnSent { get; private set; }
        public Civilization Sender { get; private set; }
        public Civilization Recipient { get; private set; }
        public StatementType StatementType { get; private set; }
        public Tone Tone { get; private set; }
        public object Parameter { get; private set; }
    }
}