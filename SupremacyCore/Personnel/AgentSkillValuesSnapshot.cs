// OfficerExperienceRatings.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.IO.Serialization;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public sealed class AgentSkillRatingsSnapshot : IOwnedDataSerializableAndRecreatable,
                                                    INotifyPropertyChanged,
                                                    ICloneable,
                                                    IKeyedLookup<AgentSkill, int>
    {
        public const int MaxValue = SerializationWriter.HighestOptimizable32BitValue;
        public const int MinValue = 0;

        private const string IndexerName = "Item[]";

        private int[] _values;

        public AgentSkillRatingsSnapshot()
        {
            _values = new int[EnumHelper.GetValues<AgentSkill>().Length];
        }

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion

        #region IKeyedLookup<AgentSkill,int> Members
        public int this[AgentSkill category]
        {
            get { return _values[(int)category]; }
            set
            {
                _values[(int)category] = Math.Min(MaxValue, Math.Max(MinValue, value));
                OnPropertyChanged(IndexerName);
            }
        }

        public bool Contains(AgentSkill key)
        {
            return true;
        }

        public IEqualityComparer<AgentSkill> KeyComparer
        {
            get { return EqualityComparer<AgentSkill>.Default; }
        }
        #endregion

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IOwnedDataSerializableAndRecreatable Members
        void IOwnedDataSerializable.SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.WriteOptimized(_values);
        }

        void IOwnedDataSerializable.DeserializeOwnedData(SerializationReader reader, object context)
        {
            _values = reader.ReadInt32Array();
            OnPropertyChanged(IndexerName);
        }
        #endregion

        #region Implementation of IEnumerable
        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            return ((IEnumerable<int>)_values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_values).GetEnumerator();
        }
        #endregion

        public bool MeetsOrExceeds(AgentSkillRatingsSnapshot categories)
        {
            if (categories == null)
                throw new ArgumentNullException("categories");

            return EnumHelper
                .GetValues<AgentSkill>()
                .All(category => this[category] >= categories[category]);
        }

        public void Add(AgentSkillRatingsSnapshot ratings)
        {
            if (ratings == null)
                throw new ArgumentNullException("ratings");

            EnumHelper
                .GetValues<AgentSkill>()
                .ForEach(o => _values[(int)o] += ratings[o]);

            OnPropertyChanged(IndexerName);
        }

        public void Clear()
        {
            Array.Clear(_values, 0, _values.Length);
            OnPropertyChanged(IndexerName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotNull]
        public AgentSkillRatingsSnapshot Clone()
        {
            var clone = new AgentSkillRatingsSnapshot();
            _values.CopyTo(clone._values, 0);
            return clone;
        }
    }
}