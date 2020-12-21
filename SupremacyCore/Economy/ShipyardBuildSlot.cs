using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Supremacy.Annotations;
using Supremacy.Collections;
using Supremacy.IO.Serialization;
using Supremacy.Orbitals;
using Supremacy.Tech;

namespace Supremacy.Economy
{
    [Serializable]
    public class ShipyardBuildSlot : BuildSlot
    {
        [NonSerialized]
        private Shipyard _shipyard;
        [NonSerialized]
        private int _slotId;
        private bool _isActive;
        private BuildPriority _priority;
        private Queue<BuildProject> _buildSlotQueue;
       // private BuildQueueItem _buildSlotQueueItem;
        private BuildProject _buildSlotQueueProject;

        public Shipyard Shipyard
        {
            get { return _shipyard; }
            set
            {
                _shipyard = value;
                OnPropertyChanged("Shipyard");
            }
        }

        public int SlotID
        {
            get { return _slotId; }
            set
            {
                _slotId = value;
                OnPropertyChanged("SlotID");
            }
        }

        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                OnPropertyChanged("IsActive");
                OnPropertyChanged("OnHold");
            }
        }


        public Queue<BuildProject> BuildSlotQueue
        {
            get
            {
                return _buildSlotQueue;
            }
        }
        //public BuildProject BuildSlotQueueItem
        //{
        //    get { return _buildSlotQueueProject; }
        //    set
        //    {
        //        value = _buildSlotQueueProject;
        //        OnPropertyChanged("BuildSlotQueue");
        //    }
        //}

        /// <summary>
        /// Gets or sets the project in Queue <see cref="ShipyardBuildSlot"/>.
        /// </summary>
        /// <value>The project in Queue.</value>
        public virtual BuildProject BuildSlotQueueProject
        {
            get { return _buildSlotQueueProject; }
            set
            {
                _buildSlotQueueProject = value;
                OnPropertyChanged("BuildSlotQueueProject");
                OnPropertyChanged("HasBuildSlotQueueProject");
            }
        }

        public bool HasBuildSlotQueueProject
        {
            get { return (_buildSlotQueueProject != null); }
        }

        [ContractInvariantMethod, UsedImplicitly]
        private void Invariants()
        {
            Contract.Invariant(HasBuildSlotQueueProject || BuildSlotQueueProject == null);
        }


        public ShipyardBuildSlot()
        {
            Initialize();
        }
        private void Initialize()
        {
            //_project = null;
            _priority = BuildPriority.Normal;
            _buildSlotQueue = new Queue<BuildProject>();
            _buildSlotQueueProject = null;
        }

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
        /// Determines whether a ship of the specified design is under construction.
        /// </summary>
        /// <param name="design">The ship design.</param>
        /// <returns>
        /// <c>true</c> if a ship of the specified design is under construction; otherwise, <c>false</c>.
        /// </returns>
        internal bool IsBuilding(TechObjectDesign design)
        {
            return this.Project != null && this.Project.BuildDesign == design ||
                   BuildSlotQueue.Any(item => item.BuildDesign == design);
        }

        public bool IsShipyardBuildSlotQueueFull(ObservableCollection<BuildQueueItem> buildSlotQueue)
        {
            bool fullQueue = false;
            if (buildSlotQueue.Count < 4)
                fullQueue = true;

                return fullQueue; ;
        }

        public void ProcessQueue()
        {
            foreach (var queueProject in BuildSlotQueue)
            {
                //if (this.HasProject && this.Project.IsCancelled)
                //    this.Project = null;
                if (BuildSlotQueue.Count >= 4)
                {
                    continue;
                }
                if (queueProject == null)
                    continue;
                
                //var buildSlotQueueProject = this.BuildSlotQueue.FirstOrDefault();
                ////var queueItemForQueueView = this.BuildSlotQueue.LastOrDefault();
                //this.Projec
                    
                //    if (BuildSlotQueue.Count() > 1)
                //    {
                //        // this.Project = queueItemForProject.CloneEquivalent();
                //        this.BuildSlotQueueProject = queueItemForProject;
                //        //BuildQueue.DecrementCount();
                //    }
                    
                //    else
                //    {
                //        this.Project = queueItemForProject;
                //        //BuildQueue.Remove(queueItem);
                //    }
                //}
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

        /// <summary>
        /// Called when the Build Queue changes.
        /// </summary>
        internal void OnBuildSlotQueueChanged()
        {
            OnPropertyChanged("BuildSlotQueue");
        }

        #endregion

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _isActive = reader.ReadBoolean();
            //_buildQueue = new ObservableCollection<BuildQueueItem>((BuildQueueItem[])reader.ReadObjectArray(typeof(BuildQueueItem)));
            // _buildQueueItem.DeserializeOwnedData(reader, context);
            //_buildQueue.AddRange((BuildQueueItem[])reader.ReadOptimizedObjectArray(typeof(BuildQueueItem)));

            //base.DeserializeOwnedData(reader, context);

            
           // _buildSlots = new ArrayWrapper<ShipyardBuildSlot>((ShipyardBuildSlot[])reader.ReadOptimizedObjectArray(typeof(ShipyardBuildSlot)));

            //UpdateBuildSlots();

        }

        public override bool OnHold
        {
            get { return HasProject && (Project.IsPaused || !IsActive); }
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            writer.Write(_isActive);
            //writer.WriteOptimized(_buildQueue.ToArray());
            //_buildQueueItem.SerializeOwnedData(writer, context);

           // writer.WriteOptimized(_buildQueue.ToArray());
            //writer.Write(_buildQueue.Cast<object>().ToArray());
           // writer.WriteOptimized(_buildSlots.ToArray());
        }
    }
}