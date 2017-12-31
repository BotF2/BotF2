// BuildSlot.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;

using Supremacy.Annotations;
using Supremacy.IO.Serialization;

namespace Supremacy.Economy
{
    /// <summary>
    /// Defines the priority levels that can be assigned to a <see cref="BuildProject"/>
    /// </summary>
    public enum BuildPriority : byte
    {
        Low = 0,
        Normal,
        High
    }

    /// <summary>
    /// Represents a single slot available for construction at a production center.
    /// </summary>
    [Serializable]
    public class BuildSlot : IOwnedDataSerializableAndRecreatable, INotifyPropertyChanged
    {
        private BuildProject _project;
        private BuildPriority _priority;

        /// <summary>
        /// Gets or sets the project under construction in this <see cref="BuildSlot"/>.
        /// </summary>
        /// <value>The project under construction.</value>
        public virtual BuildProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                OnPropertyChanged("Project");
                OnPropertyChanged("HasProject");
            }
        }

        public bool HasProject
        {
            get
            {
                return (_project != null);
            }
        }

        [ContractInvariantMethod, UsedImplicitly]
        private void Invariants()
        {
            Contract.Invariant(HasProject || Project == null);
        }

        /// <summary>
        /// Gets or sets the priority of the project under construction in this <see cref="BuildSlot"/>.
        /// </summary>
        /// <value>The project priority.</value>
        public BuildPriority Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                OnPropertyChanged("Priority");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the project under construction in this
        /// <see cref="BuildSlot"/> is on hold.
        /// </summary>
        /// <value><c>true</c> if project is on hold; otherwise, <c>false</c>.</value>
        public virtual bool OnHold
        {
            get { return HasProject && _project.IsPaused; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildSlot"/> class.
        /// </summary>
        public BuildSlot()
        {
            _project = null;
            _priority = BuildPriority.Normal;
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

        public virtual void SerializeOwnedData(SerializationWriter writer, object context)
        {
            writer.Write((byte)_priority);
            writer.WriteObject(_project);
        }

        public virtual void DeserializeOwnedData(SerializationReader reader, object context)
        {
            _priority = (BuildPriority)reader.ReadByte();
            _project = reader.Read<BuildProject>();
        }
    }
}
