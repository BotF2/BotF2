// File:Diplomat.cs
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
using Supremacy.Diplomacy.Visitors;
using Supremacy.Economy;
using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Intelligence;
using Supremacy.IO.Serialization;
using Supremacy.Universe;
using Supremacy.Utility;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class Diplomat : IOwnedDataSerializableAndRecreatable
    {
        private int _ownerId;
        private int _seatOfGovernmentId;
        private CivilizationKeyedMap<ForeignPower> _foreignPowers;
        private List<IntelHelper.NewIntelOrders> _intelOrdersGoingToHost;
        //private List<IntelHelper.NewIntelOrders> _intelOrdersGoingToHost_List;

        public int OwnerID
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
        [NotNull]
        public List<IntelHelper.NewIntelOrders> IntelOrdersGoingToHost
        {
            //get { return GetIntelOrdersGoingToHost(_ownerId); }
            get 
            {
                //works
                //if (_intelOrdersGoingToHost.Count > 0)
                    //GameLog.Server.Intel.DebugFormat("GET IntelOrdersGoingToHost.Count = {0}", _intelOrdersGoingToHost.Count);


                //return GameContext.Current.CivilizationManagers[_ownerId].IntelOrdersGoingToHost; 
                return _intelOrdersGoingToHost;
            }
            set
            {
                //works
                //GameLog.Server.Intel.DebugFormat("SET IntelOrdersGoingToHost.Count = {0}", _intelOrdersGoingToHost.Count);
                if (_intelOrdersGoingToHost != value)
                {
                    //works
                    //GameLog.Server.Intel.DebugFormat("SET IntelOrdersGoingToHost to VALUE");
                    _intelOrdersGoingToHost = value;
                }
            }

            ////[NotNull]
            //public List<IntelHelper.NewIntelOrders> IntelOrdersGoingToHost
            //{
            //    get
            //    {
            //        //        //if (_intelOrdersGoingToHost == null)
            //        //        //{
            //        //        //    var _DummyintelOrdersGoingToHost = new NewIntelOrders(0,1,"Dum","out");
            //        //        //    _DummyintelOrdersGoingToHost.AttackedCivID = 0;
            //        //        //    _DummyintelOrdersGoingToHost.AttackingCivID = 1;
            //        //        //    _DummyintelOrdersGoingToHost.Intel_Order = "StealCredits";
            //        //        //    _DummyintelOrdersGoingToHost.Intel_Order_Blamed = "Blam_out";
            //        //        //    _intelOrdersGoingToHost.Add(_DummyintelOrdersGoingToHost);
            //        //        //}

            //        //        // gameLog is to often > 10000
            //        //        //GameLog.Core.Intel.DebugFormat("...doing IntelOrdersGoingToHost");

            //        return _intelOrdersGoingToHost;
            //    }
            //    set
            //            {
            //        _intelOrdersGoingToHost = value;
            //    }
            //}
        }

        public Colony SeatOfGovernment
        {
            get
            {
                if (_seatOfGovernmentId == -1)
                    return null;

                return GameContext.Current.Universe.Objects[_seatOfGovernmentId] as Colony;
            }
            internal set { _seatOfGovernmentId = (value != null) ? value.ObjectID : -1; }
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
        public void UpdateIntelOrderList(List<IntelHelper.NewIntelOrders> orderList)
        {
            // old stuff, not working 
            // GameLog.Core.Diplomacy.DebugFormat("Adding orders, count = {0}", orderList.Count);  
            _intelOrdersGoingToHost.AddRange(orderList);
        }
        public ForeignPower GetForeignPower(ICivIdentity civilization)
        {
            if (civilization == null)
                throw new ArgumentNullException("civilization");
            EnsureForeignPower(civilization);
            return _foreignPowers[civilization.CivID];
        }

        //public List<IntelHelper.NewIntelOrders> GetIntelOrdersGoingToHost(ICivIdentity civilization)
        //{
        //    if (civilization == null)
        //        throw new ArgumentNullException("civilization");
        //    //EnsureForeignPower(civilization);
        //    _intelOrdersGoingToHost_List = new List<IntelHelper.NewIntelOrders>();

        //    _intelOrdersGoingToHost_List.AddRange(_intelOrdersGoingToHost[civilization.CivID]);

        //    return _intelOrdersGoingToHost_List;
        //}

        //public List<IntelHelper.NewIntelOrders> GetIntelOrdersGoingToHost(int civID)
        //{
        //    if (civID == null)
        //        throw new ArgumentNullException("civilization");
        //    //EnsureForeignPower(civilization);
        //    var _intelOrdersGoingToHost_List = new List<IntelHelper.NewIntelOrders>();

        //    if (_intelOrdersGoingToHost_List == null)
        //    {
        //        if (IntelOrdersGoingToHost.Count > 0)
        //        {
        //            //_intelOrdersGoingToHost_List = new List<IntelHelper.NewIntelOrders>();
        //            _intelOrdersGoingToHost_List.Add(IntelOrdersGoingToHost[civID]);
        //        }

        //    }
        //    else
        //    {
        //        if (IntelOrdersGoingToHost.Count > 0)
        //            _intelOrdersGoingToHost_List.Add(IntelOrdersGoingToHost[civID]);
        //    }

        //    GameLog.Server.Intel.DebugFormat("***List IntelOrdersGoingToHost_List.Count = {0}", _intelOrdersGoingToHost_List.Count); 
        //    return _intelOrdersGoingToHost_List;
        //    //return _intelOrdersGoingToHost[civID];
        //}

        public Diplomat(ICivIdentity owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            _ownerId = owner.CivID;
            _foreignPowers = new CivilizationKeyedMap<ForeignPower>(o => o.CounterpartyID);
            _intelOrdersGoingToHost = new List<IntelHelper.NewIntelOrders>();
            //_intelOrdersGoingToHost.Add();
            try
            {
                _intelOrdersGoingToHost = Diplomat.Get(owner)._intelOrdersGoingToHost;
            }
            catch
            {
                GameLog.Server.Intel.DebugFormat("***List _intelOrdersGoingToHost might be empty");
            }
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

            var _diplomats = GameContext.Current.Diplomats[owner.CivID];
            _diplomats._intelOrdersGoingToHost = _diplomats.IntelOrdersGoingToHost;

            foreach (var item in _diplomats.IntelOrdersGoingToHost)
            {

                GameLog.Server.Intel.DebugFormat("IntelOrdersGoingToHost: {0} for {1} VS {2} (blamed={3})", item.Intel_Order, item.AttackingCivID, item.AttackedCivID, item.Intel_Order_Blamed);
            }

            return GameContext.Current.Diplomats[owner.CivID];
        }

        public static Diplomat Get(int ownerId)
        {

            var _diplomats = GameContext.Current.Diplomats[ownerId];
            _diplomats._intelOrdersGoingToHost = _diplomats.IntelOrdersGoingToHost;

            foreach (var item in _diplomats.IntelOrdersGoingToHost)
            {
                GameLog.Server.Intel.DebugFormat("IntelOrdersGoingToHost: {0} for {1} VS {2} (blamed={3})", item.Intel_Order, item.AttackingCivID, item.AttackedCivID, item.Intel_Order_Blamed);
            }
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
            _intelOrdersGoingToHost = reader.Read<List<IntelHelper.NewIntelOrders>>();
        }

        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(_ownerId);
            writer.WriteOptimized(_seatOfGovernmentId);
            writer.WriteObject(_foreignPowers);
            writer.WriteObject(_intelOrdersGoingToHost);
        }
    }
}