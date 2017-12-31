// Officer.cs
//
// Copyright (c) 2009 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Supremacy.Annotations;
using Supremacy.IO.Serialization;

using System.Linq;

using Supremacy.Universe;

namespace Supremacy.Personnel
{
    [Serializable]
    public class Officer : UniverseObject
    {
        private OfficerExperienceRatings _experienceRatings;
        private OfficerExperienceCategory[] _naturalExperienceCategories;

        [CanBeNull]
        public string Image { get; set; }

        [NotNull]
        public IEnumerable<OfficerExperienceCategory> NaturalExperienceCategories
        {
            get
            {
                if (_naturalExperienceCategories == null)
                    _naturalExperienceCategories = new OfficerExperienceCategory[0];
                return _naturalExperienceCategories.AsEnumerable();
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _naturalExperienceCategories = value.ToArray();
                OnPropertyChanged("NaturalExperienceCategories");
            }
        }

        [NotNull]
        public OfficerExperienceRatings ExperienceRatings
        {
            get
            {
                if (_experienceRatings == null)
                    _experienceRatings = new OfficerExperienceRatings();
                return _experienceRatings;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (_experienceRatings != null)
                {
                    if (_experienceRatings == value)
                        return;
                    _experienceRatings.PropertyChanged -= OnExperienceRatingsPropertyChanged;
                }
                _experienceRatings = value;
                _experienceRatings.PropertyChanged += OnExperienceRatingsPropertyChanged;

            }
        }

        private void OnExperienceRatingsPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged("OfficerExperienceRatings");
        }

        #region IOwnedDataSerializableAndRecreatable Members
        public override void DeserializeOwnedData([NotNull] SerializationReader reader, [CanBeNull] object context)
        {
            base.DeserializeOwnedData(reader, context);
            this.Image = reader.ReadString();
            this.NaturalExperienceCategories = reader.ReadList<OfficerExperienceCategory>();
            this.ExperienceRatings = reader.Read<OfficerExperienceRatings>();
        }

        public override void SerializeOwnedData([NotNull] SerializationWriter writer, [CanBeNull] object context)
        {
            base.SerializeOwnedData(writer, context);
            writer.Write(this.Image);
            writer.Write(this.NaturalExperienceCategories.ToList());
            writer.WriteObject(this.ExperienceRatings);
        }
        #endregion
    }
}