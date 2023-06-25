// File:Shipyard.cs
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
using Supremacy.Utility;
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
        // private ArrayWrapper<BuildProject> _buildSlotQueues;
        private ObservableCollection<BuildQueueItem> _buildQueue;
        private string _text;
        private readonly string newline = Environment.NewLine;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public sealed override UniverseObjectType ObjectType => UniverseObjectType.Shipyard;

        /// <summary>
        /// Gets the shipyard design.
        /// </summary>
        /// <value>The shipyard design.</value>
        public ShipyardDesign ShipyardDesign
        {
            get => Design as ShipyardDesign;
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
            // _buildSlotQueues = new ArrayWrapper<BuildProject>(new BuildProject[design.BuildSlotQueues]);

            for (int i = 0; i < _buildSlots.Count; i++)
            {
                _buildSlots[i] = new ShipyardBuildSlot { Shipyard = this, SlotID = i };
            }

            _buildQueue = new ObservableCollection<BuildQueueItem>();
        }

        public IIndexedEnumerable<ShipyardBuildSlot> BuildSlots => _buildSlots;

        //public IIndexedEnumerable<BuildProject> BuildSlotQueues
        //{
        //    get { return _buildSlotQueues; }
        //}

        #region IProductionCenter Members
        /// <summary>
        /// Gets the build slots at this <see cref="Shipyard"/>.
        /// </summary>
        /// <value>The build slots.</value>
        IIndexedEnumerable<BuildSlot> IProductionCenter.BuildSlots => _buildSlots;

        /// <summary>
        /// Gets the build output for the specified build slot number.
        /// </summary>
        /// <param name="slot">The build slot number.</param>
        /// <returns>The build output.</returns>
        public int GetBuildOutput(int slot)
        {
            float output = ShipyardDesign.BuildSlotOutput;
            if (Sector.System.Colony != null && Sector.System.Colony.NetIndustry != 0) // to avoid crashes
            {
                switch (ShipyardDesign.BuildSlotOutputType)
                {
                    case ShipyardOutputType.PopulationRatio:
                        output = output / 100 * Sector.System.Colony.Population.CurrentValue;
                        break;
                    case ShipyardOutputType.IndustryRatio:
                        output = output / 100 * Sector.System.Colony.NetIndustry;
                        break;
                    case ShipyardOutputType.Static:
                    default:
                        break;
                }
            }

            if (ShipyardDesign.BuildSlotMaxOutput > 0)
            {
                output = Math.Min(output, ShipyardDesign.BuildSlotMaxOutput);
            }

            float shipBuildingBonus = Sector.System.Colony.Buildings
                .Where(o => o.IsActive)
                .SelectMany(o => o.BuildingDesign.Bonuses)
                .Where(o => o.BonusType == BonusType.PercentShipBuilding)
                .Select(o => o.Amount * 0.01f)
                .Sum();

            output *= 1 + shipBuildingBonus;

            return (int)output;
        }

        /// <summary>
        /// Gets the build queue at this <see cref="Shipyard"/>.
        /// </summary>
        /// <value>The build queue.</value>
        public IList<BuildQueueItem> BuildQueue => _buildQueue;

        /// <summary>
        /// Remove any completed projects from the build slots and dequeue new projects
        /// as slots become available.
        /// </summary>
        public void ProcessQueue()
        {
            int count = 0;
            //if (_buildQueue.Count > 0)
            //{
                
            //    _text = "BuildQueue.Count= " + BuildQueue.Count; 
            //    Console.WriteLine(_text);
            //    //GameLog.Client.ShipProductionDetails.DebugFormat(_text);
            //}

            foreach (BuildQueueItem buildQueueItem in BuildQueue)
            {
                _text = "buildQueueItem= " + buildQueueItem.Description + ", index " + count 
                    + ", TurnsRemaining " + buildQueueItem.TurnsRemaining 
                    + " at " + buildQueueItem.Project.Location 
                    + " at " + buildQueueItem.Project.ProductionCenter;
                Console.WriteLine(_text);
                //GameLog.Client.ShipProductionDetails.DebugFormat(_text);
                count++;
            }
            //int bays = BuildSlots.Count();
            int baysWithProjects = 0;
            foreach (ShipyardBuildSlot slot in BuildSlots)
            {
                _text = "Step_8300: checking " + slot.Shipyard.Design /*+ ", index " + count*/

                    + " Slot= " + slot.SlotID
                    + " at " + slot.Shipyard.Location
                    + ", Owner=" + slot.Shipyard.Owner

                    ;
                Console.WriteLine(_text);
                //GameLog.Client.ShipProductionDetails.DebugFormat(_text);

                if (slot.HasProject && slot.Project.IsCancelled)
                {
                    slot.Project = null;
                }

                if (slot.HasProject)
                {
                    baysWithProjects++;
                }

                BuildQueueItem queueItem = BuildQueue.FirstOrDefault();
                if (queueItem == null)
                {
                    continue;
                }

                if (slot.Project != null || slot.IsActive == false)
                {
                    continue;
                }

                if (queueItem.Count > 1)
                {
                    slot.Project = queueItem.Project.CloneEquivalent();
                    _ = queueItem.DecrementCount();
                }
                else
                {
                    slot.Project = queueItem.Project;
                    _ = BuildQueue.Remove(queueItem);
                }
            }
            int afterCount = 0;
            foreach (BuildQueueItem buildQueueItem in BuildQueue)
            {
                _text = "Shipyard before BuildQueueItem = " + buildQueueItem.Description + ", index " + count;
                Console.WriteLine(_text);
                //GameLog.Client.ShipProductionDetails.DebugFormat(_text);
                GameLog.Client.ShipProductionDetails.DebugFormat("Shipyard After BuildQueueItem = {0}, index {1}", buildQueueItem.Description, afterCount);
                afterCount++;
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

            //_text = "Step_7601: SerializeOwnedData ------------";
            //Console.WriteLine(_text);
            try
            {
                foreach (ShipyardBuildSlot slot in _buildSlots)
                {
                    string _design = "nothing";
                    string _percent = "0 %";
                    if (slot.Project != null && slot.Project.BuildDesign != null)
                    {
                        _design = slot.Project.BuildDesign.ToString();
                        _percent = slot.Project.PercentComplete.ToString();
                    }

                    if (_percent != "0 %")
                    {
                        _text = "Step_7602: Serialize " + slot.Shipyard.Location
                            + " > Slot= " + slot.SlotID
                            + " at " + slot.Shipyard.Name
                            //+ " " + 
                            + " > " + _percent
                            + " done for " + _design
                            ;
                        Console.WriteLine(_text);
                        GameLog.Core.SaveLoadDetails.DebugFormat(_text);
                    }

                }
            }
            catch { };
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _buildQueue = new ObservableCollection<BuildQueueItem>((BuildQueueItem[])reader.ReadObjectArray(typeof(BuildQueueItem)));
            _buildSlots = new ArrayWrapper<ShipyardBuildSlot>((ShipyardBuildSlot[])reader.ReadOptimizedObjectArray(typeof(ShipyardBuildSlot)));

            for (int i = 0; i < _buildQueue.Count; i++)
            {
                if (_buildQueue[i].Project != null)
                {
                    _text = "Step_7800: " + _buildQueue[i].Project.Location
                        //+ "; " + _buildQueue[i].Project.ProductionCenter   // crashes
                        //+ "; " + _buildQueue[i].Project.ProductionCenter.Owner
                        + ";Shipyard-Slot-BuildQueue;" + i
                        //+ ";" + _buildQueue[i].Project.Builder
                        + ";"
                        //+ ";" + _buildQueue[i].Project.TurnsRemaining


                    ;
                    Console.WriteLine(_text);
                }

                //if (!item.HasProject)
                //{
                //    _text += "Shipyard_buildSlot: No Project" + newline;
                //    Console.WriteLine(_text);
                //    //GameLog.Core.Stations.DebugFormat(_text);
                //}
                //else
                //{
                //    if (item.Project != null)
                //    {
                        //_text += item.Project.Builder
                        //    //+ "; " + item.Project.BuildDesign
                        //    + newline
                        //;
                        //Console.WriteLine(_text);
                        //GameLog.Core.Stations.DebugFormat(_text);
                    //}
                //}
            }


            foreach (var item in _buildSlots)
            {
                if (item.Project != null)
                {
                    _text = "Step_5860: " + item.Project.Location
                        //+ "; " + item.Project.ProductionCenter   // crashes
                        //+ "; " + item.Project.ProductionCenter.Owner
                        + ";No Project for _Shipyard._buildslots" + newline/*+ item.Project.Design*/

                    ;
                }
                //else
                //{
                //    _text = item.
                //}


                if (!item.HasProject)
                {
                    //_text += item
                    //    //+ "; " + item.Shipyard.Location// crashes
                    //    //+ "; " + item.Shipyard.Design
                    //    + " > BuildSlot: No Project" + newline;
                    //Console.WriteLine(_text);
                    //GameLog.Core.Stations.DebugFormat(_text);
                    _text += "";
                }
                else
                {
                    _text = "";
                    if (item.Project != null && item.Project.BuildDesign != null)
                    {
                        string _builder = "";
                        // still crashing....
                        //try 
                        //{ 
                        //    if (item.Project.Builder != null && item.Project.Builder.Key != null)
                        //        _builder = item.Project.Builder.Key; 
                        //} catch { }
                                
                        _text = "Step_5880: " + _builder
                            + ";Project for _Shipyard._buildslots " + item
                            + "; " + item.Project
                            + "; "
                        ;
                        Console.WriteLine(_text);
                    //GameLog.Core.Stations.DebugFormat(_text);
                    }
                    else
                    {
                        _text += "Step_5885: " 
                            + item.Project.Location 
                            + " > Slot " + item.SlotID
                            + " Shipyard._buildslots - unsure whether project..."
                            ; //   ??
                        Console.WriteLine(_text);
                        _text = "";
                    }
                }
                //_text = "";
            }

            UpdateBuildSlots();
        }

        private void UpdateBuildSlots()
        {
            if (_buildSlots == null)
            {
                return;
            }

            for (int i = 0; i < _buildSlots.Count; i++)
            {
                ShipyardBuildSlot buildSlot = _buildSlots[i];
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
