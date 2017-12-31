// Diplomat.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Annotations;
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.IO.Serialization;
using Supremacy.Universe;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class Diplomat : IOwnedDataSerializableAndRecreatable
    {
        private GameObjectID _ownerId;
        private GameObjectID _seatOfGovernmentId;
        private CivilizationKeyedMap<ForeignPower> _foreignPowers;

        public GameObjectID OwnerID
        {
            get { return _ownerId; }
        }

        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        public Treasury OwnerTreasury
        {
            get { return GameContext.Current.CivilizationManagers[_ownerId].Treasury; }
        }

        public ResourcePool OwnerResources
        {
            get { return GameContext.Current.CivilizationManagers[_ownerId].Resources; }
        }

        public Colony SeatOfGovernment
        {
            get
            {
                if (!_seatOfGovernmentId.IsValid)
                    return null;

                return GameContext.Current.Universe.Objects[_seatOfGovernmentId] as Colony;
            }
            internal set { _seatOfGovernmentId = (value != null) ? value.ObjectID : GameObjectID.InvalidID; }
        }

        public IDiplomacyData GetData(ICivIdentity civilization)
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");

            var extendedData = GetExtendedData(civilization);
            if (extendedData == null)
                return null;

            return extendedData.BaseData;
        }

        public IDiplomacyDataExtended GetExtendedData(ICivIdentity civilization)
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");
            
            var foreignPower = EnsureForeignPower(civilization);
            if (foreignPower == null)
                return null;

            return foreignPower.DiplomacyData;
        }

        public ForeignPower GetForeignPower(ICivIdentity civilization)
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");
            EnsureForeignPower(civilization);
            return _foreignPowers[civilization.CivID];
        }

        public Diplomat(ICivIdentity owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            _ownerId = owner.CivID;
            _foreignPowers = new CivilizationKeyedMap<ForeignPower>(o => o.CounterpartyID);
        }

        protected ForeignPower EnsureForeignPower(ICivIdentity counterparty)
        {
            if (counterparty == null)
                throw new ArgumentNullException("counterparty");

            if (counterparty.CivID == OwnerID)
                return null;

            ForeignPower foreignPower;

            if (!_foreignPowers.TryGetValue(counterparty.CivID, out foreignPower))
            {
                foreignPower = new ForeignPower(Owner, counterparty);
                _foreignPowers.Add(foreignPower);
            }

            return foreignPower;
        }

        public void EnsureForeignPowers()
        {
            foreach (var civ in GameContext.Current.Civilizations)
            {
                if (civ.CivID == _ownerId)
                    continue;
                EnsureForeignPower(civ);
            }
        }

        public bool CanAfford(IProposal proposal)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");

            foreach (var clause in proposal.Clauses)
            {
                var ownerTreasury = OwnerTreasury;
                var ownerResources = OwnerResources;

                switch (clause.ClauseType)
                {
                    case ClauseType.OfferGiveCredits:
                        if (proposal.Sender == Owner)
                        {
                            if (ownerTreasury.CurrentLevel < clause.GetData<int>())
                                return false;
                        }
                        break;

                    case ClauseType.RequestGiveCredits:
                        if (proposal.Recipient == Owner)
                        {
                            if (ownerTreasury.CurrentLevel < clause.GetData<int>())
                                return false;
                        }
                        break;

                    case ClauseType.OfferGiveResources:
                        if (proposal.Sender == Owner)
                        {
                            var resources = clause.GetData<ResourceValueCollection>();
                            if (resources != null && !ownerResources.MeetsOrExceeds(resources))
                                return false;
                        }
                        break;

                    case ClauseType.RequestGiveResources:
                        if (proposal.Recipient == Owner)
                        {
                            var resources = clause.GetData<ResourceValueCollection>();
                            if (resources != null && !ownerResources.MeetsOrExceeds(resources))
                                return false;
                        }
                        break;
                }
            }
            return true;
        }

        public void AcceptProposal([NotNull] IProposal proposal)
        {
            if (proposal == null)
                throw new ArgumentNullException("proposal");
            if (proposal.Recipient.CivID != _ownerId)
                throw new ArgumentException("Cannot accept a proposal which was not sent to this civilization!");
            if (proposal.Sender.CivID == _ownerId)
                throw new ArgumentException("Cannot accept a proposal sent by ourselves!");

            AcceptProposalVisitor.Visit(proposal);
        }

        public static Diplomat Get([NotNull] ICivIdentity owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            return GameContext.Current.Diplomats[owner.CivID];
        }

        public static Diplomat Get(GameObjectID ownerId)
        {
            return GameContext.Current.Diplomats[ownerId];
        }

        public static Diplomat Get([NotNull] string ownerKey)
        {
            if (ownerKey == null)
                throw new ArgumentNullException("ownerKey");

            return GameContext.Current.Diplomats[ownerKey];
        }

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _ownerId = reader.ReadOptimizedInt32();
            _seatOfGovernmentId = reader.ReadOptimizedInt32();
            _foreignPowers = reader.Read<CivilizationKeyedMap<ForeignPower>>();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(_ownerId);
            writer.WriteOptimized(_seatOfGovernmentId);
            writer.WriteObject(_foreignPowers);
        }
    }
}