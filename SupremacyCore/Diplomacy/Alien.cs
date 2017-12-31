// Alien.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Linq;

using Supremacy.Entities;
using Supremacy.Game;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class Alien
    {
        public GameObjectID OwnerID { get; private set; }
        public GameObjectID CivilizationID { get; private set; }
        public bool IsEmbargoInPlace { get; private set; }
        public IProposal ProposalSent { get; set; }
        public IProposal ProposalReceived { get; set; }
        public IProposal LastProposalSent { get; set; }
        public IProposal LastProposalReceived { get; set; }
        public IResponse ResponseSent { get; set; }
        public IResponse ResponseReceived { get; set; }
        public IResponse LastResponseSent { get; set; }
        public IResponse LastResponseReceived { get; set; }

        public Alien(Civilization owner, Civilization civilization)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (civilization == null)
                throw new ArgumentNullException("civilization");
            this.Owner = owner;
            this.Civilization = civilization;
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[OwnerID]; }
            private set { OwnerID = (value != null) ? value.CivID : Civilization.InvalidID; }
        }

        public Civilization Civilization
        {
            get { return GameContext.Current.Civilizations[CivilizationID]; }
            private set { CivilizationID = (value != null) ? value.CivID : Civilization.InvalidID; }
        }

        public void BeginEmbargo()
        {
            // TODO: Break any trade agreements
            this.IsEmbargoInPlace = true;
        }

        public void EndEmbargo()
        {
            this.IsEmbargoInPlace = false;
        }
    }
}