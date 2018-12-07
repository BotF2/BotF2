// CivilizationPairedMap.cs
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

using Supremacy.Game;
using Supremacy.IO.Serialization;

namespace Supremacy.Entities
{
    /// <summary>
    /// Represents a collection of values keyed by a pair of two <see cref="Civilization"/>s,
    /// where the first <see cref="Civilization"/> is considered the owner.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of item contained in the <see cref="CivilizationPairedMap&lt;TValue&gt;"/>.
    /// </typeparam>
    [Serializable]
    public sealed class CivilizationPairedMap<TValue> : IOwnedDataSerializableAndRecreatable, IEnumerable<TValue>
    {
        private Dictionary<int, TValue> _map;

        public CivilizationPairedMap()
        {
            Initialize();
        }

        private void Initialize()
        {
            _map = new Dictionary<int, TValue>();
        }

        private static int GetKey(int ownerCivId, int otherCivId)
        {
            return ((ownerCivId << 16) | otherCivId);
        }

        public void Add(Civilization ownerCiv, Civilization otherCiv, TValue value)
        {
            this[ownerCiv, otherCiv] = value;
        }

        public bool Remove(Civilization ownerCiv, Civilization otherCiv)
        {
            if ((ownerCiv == null) || (otherCiv == null))
                return false;
            return _map.Remove(GetKey(ownerCiv.CivID, otherCiv.CivID));
        }

        public bool HasValue(Civilization ownerCiv, Civilization otherCiv)
        {
            if ((ownerCiv == null) || (otherCiv == null))
                return false;
            return _map.ContainsKey(GetKey(ownerCiv.CivID, otherCiv.CivID));
        }

        public TValue this[Civilization ownerCiv, Civilization otherCiv]
        {
            get
            {
                if (ownerCiv == null)
                    throw new ArgumentNullException("ownerCiv");
                if (otherCiv == null)
                    throw new ArgumentNullException("otherCiv");
                return this[ownerCiv.CivID, otherCiv.CivID];
            }
            set
            {
                if (ownerCiv == null)
                    throw new ArgumentNullException("ownerCiv");
                if (otherCiv == null)
                    throw new ArgumentNullException("otherCiv");
                this[ownerCiv.CivID, otherCiv.CivID] = value;
            }
        }

        public TValue this[int ownerCivId, int otherCivId]
        {
            get
            {
                TValue result;
                if (_map.TryGetValue(GetKey(ownerCivId, otherCivId), out result))
                    return result;
                return default(TValue);
            }
            set
            {
                _map.Remove(GetKey(ownerCivId, otherCivId));
                if (!Equals(value, null))
                    _map[GetKey(ownerCivId, otherCivId)] = value;
            }
        }

        public IEnumerable<TValue> GetValuesForOwner(Civilization ownerCiv)
        {
            foreach (var otherCiv in GameContext.Current.Civilizations)
            {
                if (otherCiv == ownerCiv)
                    continue;

                TValue value;

                if (TryGetValue(ownerCiv, otherCiv, out value))
                    yield return value;
            }
        }

        public bool TryGetValue(Civilization firstCiv, Civilization secondCiv, out TValue result)
        {
            if (firstCiv == null || secondCiv == null)
            {
                result = default(TValue);
                return false;
            }

            return TryGetValue(firstCiv.CivID, secondCiv.CivID, out result);
        }

        public bool TryGetValue(int firstCivId, int secondCivId, out TValue result)
        {
            return _map.TryGetValue(GetKey(firstCivId, secondCivId), out result);
        }

        #region IOwnedDataSerializable Members

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_map.Count);
            foreach (var key in _map.Keys)
            {
                writer.Write(key);
                writer.WriteObject(_map[key]);
            }
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            Initialize();

            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
                _map.Add(reader.ReadInt32(), reader.Read<TValue>());
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var pair in _map)
                yield return pair.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}