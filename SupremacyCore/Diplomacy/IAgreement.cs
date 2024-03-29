﻿// IAgreement.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Annotations;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Collections;

namespace Supremacy.Diplomacy
{
    public interface IDiplomaticExchange
    {
        Civilization Sender { get; }
        Civilization Recipient { get; }
    }

    public interface IAgreement : IDiplomaticExchange
    {
        int SenderID { get; }
        int RecipientID { get; }
        int StartTurn { get; }
        int EndTurn { get; }
        IProposal Proposal { get; }
        IDictionary<object, object> Data { get; }
    }

    [Serializable]
    public class NewAgreement : IAgreement
    {
        private readonly IProposal _proposal;
        private readonly int _startTurn;
        private readonly IDictionary<object, object> _data;
        private int _endTurn;

        public NewAgreement([NotNull] IProposal proposal, int startTurn, IDictionary<object, object> data)
        {
            _proposal = proposal ?? throw new ArgumentNullException("proposal");
            _startTurn = startTurn;
            _endTurn = 0;
            _data = data;
        }

        #region Implementation of IAgreement

        public int SenderID => Proposal.Sender.CivID;

        public int RecipientID => Proposal.Recipient.CivID;

        public Civilization Sender => GameContext.Current.Civilizations[SenderID];

        public Civilization Recipient => GameContext.Current.Civilizations[RecipientID];

        public int StartTurn => _startTurn;

        public int EndTurn => _endTurn;

        public IProposal Proposal => _proposal;

        public IDictionary<object, object> Data
        {
            get
            {
                if (_data == null)
                {
                    return null;
                }

                return _data.AsReadOnly();
            }
        }

        #endregion

        public void End()
        {
            if (_endTurn == 0)
            {
                _endTurn = GameContext.Current.TurnNumber;
            }
        }
    }
}
