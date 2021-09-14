// AgreementMatrix.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.Entities;
using Supremacy.IO.Serialization;

namespace Supremacy.Diplomacy
{
    [Serializable]
    public class AgreementMatrix : IOwnedDataSerializableAndRecreatable, IEnumerable<IAgreement>
    {
        private CivilizationPairedMap<IIndexedCollection<IAgreement>> _map;

        public AgreementMatrix()
        {
            Initialize();
        }

        public void AddAgreement(IAgreement agreement)
        {
            if (agreement == null)
            {
                throw new ArgumentNullException("agreement");
            }

            int firstCivId = agreement.SenderID;
            int secondCivId = agreement.RecipientID;

            ReorderCivIds(ref firstCivId, ref secondCivId);

            if (!(_map[firstCivId, secondCivId] is CollectionBase<IAgreement> activeAgreements))
            {
                activeAgreements = new CollectionBase<IAgreement>();
                _map[firstCivId, secondCivId] = activeAgreements;
            }

            activeAgreements.Add(agreement);
        }

        public bool IsAgreementActive(ICivIdentity firstCiv, ICivIdentity secondCiv, ClauseType clauseType)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }

            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }

            return IsAgreementActive(firstCiv.CivID, secondCiv.CivID, clauseType);
        }

        public bool IsAgreementActive(int firstCivId, int secondCivId, ClauseType clauseType)
        {
            return this[firstCivId, secondCivId].Any(a => a.Proposal.Clauses.Any(c => c.ClauseType == clauseType));
        }

        public IAgreement FindAgreement([NotNull] ICivIdentity firstCiv, [NotNull] ICivIdentity secondCiv, ClauseType clauseType)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }

            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }

            return FindAgreement(firstCiv.CivID, secondCiv.CivID, clauseType);
        }

        public IAgreement FindAgreement(int firstCivId, int secondCivId, ClauseType clauseType)
        {
            return this[firstCivId, secondCivId].FirstOrDefault(a => a.Proposal.Clauses.Any(c => c.ClauseType == clauseType));
        }

        public IAgreement FindAgreement([NotNull] ICivIdentity firstCiv, [NotNull] ICivIdentity secondCiv, Func<IAgreement, bool> predicate)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }

            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }

            return FindAgreement(firstCiv.CivID, secondCiv.CivID, predicate);
        }

        public IAgreement FindAgreement(int firstCivId, int secondCivId, Func<IAgreement, bool> predicate)
        {
            return this[firstCivId, secondCivId].FirstOrDefault(predicate);
        }

        public void Remove(IAgreement agreement)
        {
            if (agreement == null)
            {
                return;
            }

            Remove(agreement.SenderID, agreement.RecipientID, agreement.Equals);
        }

        public void Remove(Civilization firstCiv, Civilization secondCiv, Func<IAgreement, bool> predicate)
        {
            if (firstCiv == null)
            {
                throw new ArgumentNullException("firstCiv");
            }

            if (secondCiv == null)
            {
                throw new ArgumentNullException("secondCiv");
            }

            Remove(firstCiv.CivID, secondCiv.CivID, predicate);
        }

        public void Remove(int firstCivId, int secondCivId, Func<IAgreement, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            ReorderCivIds(ref firstCivId, ref secondCivId);

            if (_map[firstCivId, secondCivId] is CollectionBase<IAgreement> agreements)
            {
                _ = agreements.RemoveWhere(predicate);
            }
        }

        public IIndexedCollection<IAgreement> this[ICivIdentity firstCiv, ICivIdentity secondCiv]
        {
            get
            {
                if (firstCiv == null)
                {
                    throw new ArgumentNullException("firstCiv");
                }

                if (secondCiv == null)
                {
                    throw new ArgumentNullException("secondCiv");
                }

                return this[firstCiv.CivID, secondCiv.CivID];
            }
        }

        public IIndexedCollection<IAgreement> this[int firstCivId, int secondCivId]
        {
            get
            {

                ReorderCivIds(ref firstCivId, ref secondCivId);

                if (!_map.TryGetValue(firstCivId, secondCivId, out IIndexedCollection<IAgreement> value))
                {
                    value = ArrayWrapper<IAgreement>.Empty;
                }

                return value;
            }
        }

        protected void ReorderCivIds(ref int firstCivId, ref int secondCivId)
        {
            if (secondCivId >= firstCivId)
            {
                return;
            }

            int temp = firstCivId;
            firstCivId = secondCivId;
            secondCivId = temp;
        }

        #region IOwnedDataSerializable Members

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            _map.SerializeOwnedData(writer, context);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            Initialize();
            _map.DeserializeOwnedData(reader, context);
        }

        private void Initialize()
        {
            _map = new CivilizationPairedMap<IIndexedCollection<IAgreement>>();
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<IAgreement> GetEnumerator()
        {
            return _map.SelectMany(o => o).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}