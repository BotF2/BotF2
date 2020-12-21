// Shipyard.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.IO.Serialization;
using Supremacy.Tech;
using Supremacy.Universe;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Represents a shipyard in the game.
    /// </summary>
    [Serializable]
    public class Shipyard : TechObject, IProductionCenter
    {
        private ArrayWrapper<ShipyardBuildSlot> _buildSlots;
        private ArrayWrapper<BuildProject> _buildSlotQueues;
        private ObservableCollection<BuildQueueItem> _buildQueue;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public sealed override UniverseObjectType ObjectType
        {
            get { return UniverseObjectType.Shipyard; }
        }

        /// <summary>
        /// Gets the shipyard design.
        /// </summary>
        /// <value>The shipyard design.</value>
        public ShipyardDesign ShipyardDesign
        {
            get { return Design as ShipyardDesign; }
            set
            {
                Design = value;
                OnPropertyChanged("ShipyardDesign");
            }
        }

        public Shipyard() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shipyard"/> class using the specified design.
        /// </summary>
        /// <param name="design">The design.</param>
        public Shipyard(ShipyardDesign design)
            : base(design)
        {
            _buildSlots = new ArrayWrapper<ShipyardBuildSlot>(new ShipyardBuildSlot[design.BuildSlots]);
            _buildSlotQueues = new ArrayWrapper<BuildProject>(new BuildProject[design.BuildSlotQueues]);
            
            for (var i = 0; i < _buildSlots.Count; i++)
                _buildSlots[i] = new ShipyardBuildSlot { Shipyard = this, SlotID = i};

            _buildQueue = new ObservableCollection<BuildQueueItem>();
        }

        public IIndexedEnumerable<ShipyardBuildSlot> BuildSlots
        {
            get { return _buildSlots; }
        }

        public IIndexedEnumerable<BuildProject> BuildSlotQueues
        {
            get { return _buildSlotQueues; }
        }

        #region IProductionCenter Members
        /// <summary>
        /// Gets the build slots at this <see cref="Shipyard"/>.
        /// </summary>
        /// <value>The build slots.</value>
        IIndexedEnumerable<BuildSlot> IProductionCenter.BuildSlots
        {
            get { return _buildSlots; }
        }

        /// <summary>
        /// Gets the build output for the specified build slot number.
        /// </summary>
        /// <param name="slot">The build slot number.</param>
        /// <returns>The build output.</returns>
        public int GetBuildOutput(int slot)
        {
            var output = (float)ShipyardDesign.BuildSlotOutput;
            switch (ShipyardDesign.BuildSlotOutputType)
            {
                case ShipyardOutputType.PopulationRatio:
                    output = (output / 100) * Sector.System.Colony.Population.CurrentValue;
                    break;
                case ShipyardOutputType.IndustryRatio:
                    output = (output / 100) * Sector.System.Colony.NetIndustry;
                    break;
                case ShipyardOutputType.Static:
                default:
                    break;
            }

            if(ShipyardDesign.BuildSlotMaxOutput > 0)
                output = Math.Min(output, ShipyardDesign.BuildSlotMaxOutput);

            var shipBuildingBonus = Sector.System.Colony.Buildings
                .Where(o => o.IsActive)
                .SelectMany(o => o.BuildingDesign.Bonuses)
                .Where(o => o.BonusType == BonusType.PercentShipBuilding)
                .Select(o => o.Amount * 0.01f)
                .Sum();

            output *= (1 + shipBuildingBonus);

            return (int)output;
        }

        /// <summary>
        /// Gets the build queue at this <see cref="Shipyard"/>.
        /// </summary>
        /// <value>The build queue.</value>
        public IList<BuildQueueItem> BuildQueue
        {
            get { return _buildQueue; }
        }

        /// <summary>
        /// Remove any completed projects from the build slots and dequeue new projects
        /// as slots become available.
        /// </summary>
        public void ProcessQueue()
        {
            foreach (var slot in BuildSlots)
            {
                if (slot.HasProject && slot.Project.IsCancelled)
                    slot.Project = null;

                if (slot.Project != null)
                    continue;

                var queueItem = BuildQueue.FirstOrDefault();
                if (queueItem == null)
                    continue;

                if (queueItem.Count > 1)
                {
                    slot.Project = queueItem.Project.CloneEquivalent();
                    queueItem.DecrementCount();
                }
                else
                {
                    slot.Project = queueItem.Project;
                    BuildQueue.Remove(queueItem);
                }
            }
        }
        #endregion

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            UpdateBuildSlots();
        }

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);
            writer.Write(_buildQueue.Cast<object>().ToArray());
            writer.WriteOptimized(_buildSlots.ToArray());
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _buildQueue = new ObservableCollection<BuildQueueItem>((BuildQueueItem[])reader.ReadObjectArray(typeof(BuildQueueItem)));
            _buildSlots = new ArrayWrapper<ShipyardBuildSlot>((ShipyardBuildSlot[])reader.ReadOptimizedObjectArray(typeof(ShipyardBuildSlot)));

            UpdateBuildSlots();
        }

        private void UpdateBuildSlots()
        {
            if (_buildSlots == null)
                return;

            for (var i = 0; i < _buildSlots.Count; i++)
            {
                var buildSlot = _buildSlots[i];
                buildSlot.Shipyard = this;
                buildSlot.SlotID = i;
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
            return BuildSlots.Any(t => t.Project != null && t.Project.BuildDesign == design) ||
                   BuildQueue.Any(item => item.Project.BuildDesign == design);
        }
    }
}
