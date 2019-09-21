// Personnel.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

using Supremacy.Entities;
using Supremacy.Game;
using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Economy
{
    /// <summary>
    /// Defines the categories of personnel used in the game.
    /// </summary>
    public enum PersonnelCategory : byte
    {
        Officers = 0,
        InternalAffairs = 1,
        ExternalAffairs = 2,
    }

    /// <summary>
    /// Represents a civilization's pool of available personnel.
    /// </summary>
    [Serializable]
    public sealed class PersonnelPool : INotifyPropertyChanged
    {
        private readonly DistributionGroup<PersonnelCategory> _distribution;
        private readonly int _ownerId;
        private readonly Meter[] _totalValues;

        /// <summary>
        /// Gets the distribution by category of newly allocated personnel for this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The distribution.</value>
        public DistributionGroup<PersonnelCategory> Distribution
        {
            get { return _distribution; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonnelPool"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public PersonnelPool(Civilization owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            var categories = EnumUtilities.GetValues<PersonnelCategory>();

            _ownerId = owner.CivID;
            _distribution = new DistributionGroup<PersonnelCategory>(categories);
            _distribution.PropertyChanged += OnDistributionPropertyChanged;
            _totalValues = new Meter[categories.Count];

            for (var i = 0; i < _totalValues.Length; i++)
                _totalValues[i] = new Meter();
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Distribution control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnDistributionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Distribution");
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
