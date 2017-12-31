// OfficerExperienceRatings.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

using Supremacy.IO.Serialization;
using Supremacy.Utility;

namespace Supremacy.Personnel
{
    [Serializable]
    public sealed class OfficerExperienceRatings : IOwnedDataSerializableAndRecreatable, INotifyPropertyChanged, ICloneable
    {
        public const int MaxValue = int.MaxValue;
        public const int MinValue = 0;

        private int[] _values;

        public int this[OfficerExperienceCategory category]
        {
            get { return _values[(int)category]; }
            set
            {
                _values[(int)category] = Math.Min(MaxValue, Math.Max(MinValue, value));
                OnPropertyChanged("Item");
            }
        }

        public OfficerExperienceRatings()
        {
            _values = new int[EnumHelper.GetValues<OfficerExperienceCategory>().Length];
        }

        public bool MeetsOrExceeds(OfficerExperienceRatings categories)
        {
            if (categories == null)
                throw new ArgumentNullException("categories");
            foreach (var category in EnumHelper.GetValues<OfficerExperienceCategory>())
            {
                if (this[category] < categories[category])
                    return false;
            }
            return true;
        }

        public void Add(OfficerExperienceRatings ratings)
        {
            if (ratings == null)
                throw new ArgumentNullException("ratings");
            foreach (var category in EnumHelper.GetValues<OfficerExperienceCategory>())
                this[category] += ratings[category];
        }

        public void Clear()
        {
            foreach (var category in EnumHelper.GetValues<OfficerExperienceCategory>())
                this[category] = 0;
        }

        #region INotifyPropertyChanged Members
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region ICloneable Members
        object ICloneable.Clone()
        {
            return Clone();
        }

        public OfficerExperienceRatings Clone()
        {
            var clone = new OfficerExperienceRatings();
            _values.CopyTo(clone._values, 0);
            return clone;
        }
        #endregion

        public void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write(_values);
        }

        public void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _values = reader.ReadInt32Array();
        }
    }
}