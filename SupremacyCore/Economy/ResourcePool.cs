// ResourcePool.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

using Supremacy.Types;
using Supremacy.Utility;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a civlization's global resource pool in the game.
    /// </summary>
    [Serializable]
    public class ResourcePool : INotifyPropertyChanged
    {
        private Meter[] _values;

        /// <summary>
        /// Gets the Dilithium stockpile.
        /// </summary>
        /// <value>The Dilithium stockpile.</value>
        public Meter Dilithium
        {
            get { return _values[(int)ResourceType.Dilithium]; }
        }

        /// <summary>
        /// Gets the Deuterium stockpile.
        /// </summary>
        /// <value>The Deuterium stockpile.</value>
        public Meter Deuterium
        {
            get { return _values[(int)ResourceType.Deuterium]; }
        }

        /// <summary>
        /// Gets the Raw Materials stockpile.
        /// </summary>
        /// <value>The Raw Materials stockpile.</value>
        public Meter RawMaterials
        {
            get { return _values[(int)ResourceType.RawMaterials]; }
        }

        /// <summary>
        /// Gets the stockpile for the specified resource type.
        /// </summary>
        /// <value>The stockpile.</value>
        public Meter this[ResourceType resource]
        {
            get { return _values[(int)resource]; }
        }

        public bool MeetsOrExceeds(ResourceValueCollection resources)
        {
            if (resources == null)
                throw new ArgumentNullException("resources");
            foreach (ResourceType resource in EnumHelper.GetValues<ResourceType>())
            {
                if (this[resource].CurrentValue < resources[resource])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePool"/> class.
        /// </summary>
        public ResourcePool()
        {
            var resources = EnumUtilities.GetValues<ResourceType>();

            _values = new Meter[resources.Count];

            for (var i = 0; i < _values.Length; i++)
            {
                _values[i] = new Meter(0, 0, Meter.MaxValue);
                _values[i].PropertyChanged += OnMeterChanged;
            }
        }

        /// <summary>
        /// Called when the value of one of the meters changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        void OnMeterChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == Dilithium)
                OnPropertyChanged("Dilithium");
            else if (sender == Deuterium)
                OnPropertyChanged("Deuterium");
            else if (sender == RawMaterials)
                OnPropertyChanged("RawMaterials");
        }

        /// <summary>
        /// Updates the and reset all of the meters.
        /// </summary>
        public void UpdateAndReset()
        {
            for (int i = 0; i < _values.Length; i++)
            {
                _values[i].UpdateAndReset();
            }
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
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
