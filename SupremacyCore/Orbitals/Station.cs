// File:Station.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;
using System.Collections.Generic;

using Supremacy.Collections;
using Supremacy.Economy;
using Supremacy.IO.Serialization;
using Supremacy.Universe;

using System.Linq;

namespace Supremacy.Orbitals
{
    /// <summary>
    /// Represents a space station in the game.
    /// </summary>
    [Serializable]
    public class Station : Orbital, IProductionCenter
    {
        private IIndexedEnumerable<BuildSlot> _buildSlots;
        private int _buildOutput;
        private List<BuildQueueItem> _buildQueue;
        private string _text;

        /// <summary>
        /// Gets the type of the UniverseObject.
        /// </summary>
        /// <value>The type of the UniverseObject.</value>
        public sealed override UniverseObjectType ObjectType => UniverseObjectType.Station;

        /// <summary>
        /// Gets or sets the station design.
        /// </summary>
        /// <value>The station design.</value>
        public StationDesign StationDesign
        {
            get => Design as StationDesign;
            set => Design = value;
        }

        public Station() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Station"/> class using the specified design.
        /// </summary>
        /// <param name="design">The design.</param>
        public Station(StationDesign design)
            : base(design)
        {
            _buildQueue = new List<BuildQueueItem>();
            _buildSlots = new ArrayWrapper<BuildSlot>(new BuildSlot[design.BuildSlots]);
            _buildOutput = design.BuildOutput;
        }

        #region IProductionCenter Members
        /// <summary>
        /// Gets the build slots at this <see cref="Station"/>.
        /// </summary>
        /// <value>The build slots.</value>
        public IIndexedEnumerable<BuildSlot> BuildSlots => _buildSlots;

        /// <summary>
        /// Gets the build output for the specified build slot number.
        /// </summary>
        /// <param name="slot">The build slot number.</param>
        /// <returns>The build output.</returns>
        public int GetBuildOutput(int slot)
        {
            return _buildOutput;
        }

        /// <summary>
        /// Gets the build queue at this <see cref="Station"/>.
        /// </summary>
        /// <value>The build queue.</value>
        public IList<BuildQueueItem> BuildQueue => _buildQueue;

        /// <summary>
        /// Remove any completed projects from the build slots and dequeue new projects
        /// as slots become available.
        /// </summary>
        public void ProcessQueue() { }
        #endregion

        public override void SerializeOwnedData(SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);
            writer.WriteOptimized(_buildSlots.ToArray());
            writer.WriteOptimized(_buildOutput);
            writer.Write(_buildQueue);
        }

        public override void DeserializeOwnedData(SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);
            _buildSlots = new ArrayWrapper<BuildSlot>((BuildSlot[])reader.ReadOptimizedObjectArray(typeof(BuildSlot)));
            _buildOutput = reader.ReadOptimizedInt32();
            _buildQueue = reader.ReadList<BuildQueueItem>();


            foreach (var item in _buildSlots)
            {
                if (item == null)
                    continue;

                if (!item.HasProject)
                {
                    _text = "Station_buildSlot: No Project";
                    Console.WriteLine(_text);
                    //GameLog.Core.Stations.DebugFormat(_text);
                }
                else
                {
                    _text = item.Project.ProductionCenter.Location
                        + item.Project.ProductionCenter.ToString()
                        ;
                    Console.WriteLine(_text);
                    //GameLog.Core.Stations.DebugFormat(_text);
                }
            }
        }
    }
}
