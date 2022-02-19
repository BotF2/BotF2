// File:BuildQueueItem.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.ComponentModel;

using Supremacy.IO.Serialization;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents an entry in a production center's build queue.
    /// </summary>
    [Serializable]
    public class BuildSlotQueueItem : BuildQueueItem
    {
        private BuildProject _project;
        private int _count;

        /// <summary>
        /// Gets the number of equivalent projects that are enqueued.
        /// </summary>
        /// <value>The number of equivalent projects.</value>
        //public int Count
        //{
        //    get { return _count; }
        //}

        //public string Description
        //{
        //    get
        //    {
        //        if (Project == null)
        //            return null;

        //        if (_count > 1)
        //            return String.Format("{0}x {1}", _count, Project.Description);

        //        return Project.Description;
        //    }
        //}

        /// <summary>
        /// Gets the queued project.
        /// </summary>
        /// <value>The queued project.</value>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public BuildProject Project => _project;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        /// Gets the total number of turns remaining until all items in this entry are completed.
        /// </summary>
        /// <value>The turns remaining.</value>
        //public int TurnsRemaining
        //{
        //    get { return _project.TurnsRemaining * _count; }
        //}

        /// <summary>
        /// When called notifies all listeners that the number of turns remaining has changed
        /// </summary>
        //public void InvalidateTurnsRemaining()
        //{
        //    OnPropertyChanged("TurnsRemaining");
        //}

        public BuildSlotQueueItem() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildQueueItem"/> class.
        /// </summary>
        /// <param name="project">The queued project.</param>
        public BuildSlotQueueItem(BuildProject project)
        {
            _project = project ?? throw new ArgumentNullException("project");
            _count = 1;
            OnPropertyChanged("Count");
            OnPropertyChanged("Project");
            OnPropertyChanged("TurnsRemaining");
        }

        /// <summary>
        /// Increments the <see cref="Count"/> property.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        //public bool IncrementCount()
        //{
        //    if (_project.CloneEquivalent() != null)
        //    {
        //        _count++;
        //        OnPropertyChanged("Count");
        //        OnPropertyChanged("Description");
        //        OnPropertyChanged("TurnsRemaining");
        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// Decrements the <see cref="Count"/> property.
        /// </summary>
        /// <returns><c>true</c> if successful; otherwise, <c>false</c>.</returns>
        //public bool DecrementCount()
        //{
        //    if (_count > 1)
        //    {
        //        _count--;
        //        OnPropertyChanged("Count");
        //        OnPropertyChanged("Description");
        //        OnPropertyChanged("TurnsRemaining");
        //        return true;
        //    }
        //    return false;
        //}

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        protected void OnPropertyChanged(string propertyName)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public void SerializeOwnedData(SerializationWriter writer, object context)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            writer.WriteOptimized(_count);
            writer.WriteObject(_project);
        }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public void DeserializeOwnedData(SerializationReader reader, object context)
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
        {
            _count = reader.ReadOptimizedInt32();
            _project = reader.Read<BuildProject>();
        }
    }
}
