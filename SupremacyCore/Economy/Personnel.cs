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
        /// Gets the owner of this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The owner.</value>
        public Civilization Owner
        {
            get { return GameContext.Current.Civilizations[_ownerId]; }
        }

        /// <summary>
        /// Gets the Officer personnel in this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The Officer personnel.</value>
        public Meter Officers
        {
            get { return this[PersonnelCategory.Officers]; }
        }

        /// <summary>
        /// Gets the Internal Affairs personnel in this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The Internal Affairs personnel.</value>
        public Meter InternalAffairs
        {
            get { return this[PersonnelCategory.InternalAffairs]; }
        }

        /// <summary>
        /// Gets the External Affairs personnel in this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The External Affairs personnel.</value>
        public Meter ExternalAffairs
        {
            get { return this[PersonnelCategory.ExternalAffairs]; }
        }

        /// <summary>
        /// Gets the personnel of the specified category in this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The personnel of the specified category.</value>
        public Meter this[PersonnelCategory category]
        {
            get { return _totalValues[(int)category]; }
        }

        /// <summary>
        /// Gets the distribution by category of newly allocated personnel for this <see cref="PersonnelPool"/>.
        /// </summary>
        /// <value>The distribution.</value>
        public DistributionGroup<PersonnelCategory> Distribution
        {
            get { return _distribution; }
        }

        /// <summary>
        /// Gets the total External Affairs bonus.
        /// </summary>
        /// <value>The total External Affairs bonus.</value>
        public Percentage ExternalAffairsBonus
        {
            get { return GetBonusTotal(BonusType.PercentExternalAffairs); }
        }

        /// <summary>
        /// Gets the total Internal Affairs bonus.
        /// </summary>
        /// <value>The total Internal Affairs bonus.</value>
        public Percentage InternalAffairsBonus
        {
            get { return GetBonusTotal(BonusType.PercentInternalAffairs); }
        }

        /// <summary>
        /// Gets the total personnel loyalty bonus.
        /// </summary>
        /// <value>The total personnel loyalty bonus.</value>
        public Percentage PersonnelLoyaltyBonus
        {
            get { return GetBonusTotal(BonusType.PercentPersonnelLoyalty); }
        }

        /// <summary>
        /// Gets the total bribe resistance bonus.
        /// </summary>
        /// <value>The total bribe resistance bonus.</value>
        public Percentage BribeResistanceBonus
        {
            get { return GetBonusTotal(BonusType.PercentBribeResistanceEmpireWide); }
        }

        /// <summary>
        /// Gets the total bonus of the specified type.
        /// </summary>
        /// <param name="bonusType">Type of the bonus.</param>
        /// <returns></returns>
        private Percentage GetBonusTotal(BonusType bonusType)
        {
            Percentage result = 0.0f;
            
            var civManager = GameContext.Current.CivilizationManagers[_ownerId];
            if (civManager == null)
                return result;
            
            foreach (var bonus in civManager.GlobalBonuses)
            {
                if (bonus.BonusType == bonusType)
                    result += (0.01f * bonus.Amount);
            }

            return result;
        }

        /// <summary>
        /// Adds the personnel to this <see cref="PersonnelPool"/>.  The personnel
        /// are allocated to the individual categories based on the values in the
        /// <see cref="Distribution"/> property.
        /// </summary>
        /// <param name="amount">The amount of personnel to add.</param>
        public void AddPersonnel(int amount)
        {
            _distribution.TotalValue = amount;

            foreach (var category in EnumUtilities.GetValues<PersonnelCategory>())
                this[category].AdjustCurrent(_distribution.Values[category]);
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

        /// <summary>
        /// Updates the and resets the <see cref="Meter"/>s for each category of personnel.
        /// </summary>
        public void UpdateAndReset()
        {
            for (var i = 0; i < _totalValues.Length; i++)
                _totalValues[i].UpdateAndReset();
        }
    }
}
