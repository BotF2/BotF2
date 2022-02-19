// StationBuildProject.cs
//
// Copyright (c) 2007 Mike Strobel
//
// This source code is subject to the terms of the Microsoft Reciprocal License (Ms-RL).
// For details, see <http://www.opensource.org/licenses/ms-rl.html>.
//
// All other rights reserved.

using System;

using Supremacy.Game;
using Supremacy.Orbitals;
using Supremacy.Resources;
using Supremacy.Utility;

namespace Supremacy.Economy
{
    /// <summary>
    /// Represents a space station construction project in the game.
    /// </summary>
    [Serializable]
    public class StationBuildProject : BuildProject
    {
        private int _productionCenterId;

        public override IProductionCenter ProductionCenter
        {
            get
            {
                Fleet fleet = GameContext.Current.Universe.Objects[_productionCenterId] as Fleet;
                if (fleet != null)
                {
                    return new BuildStationOrder.FleetProductionCenter(fleet);
                }

                return default;
            }
        }

        /// <summary>
        /// Gets the station design.
        /// </summary>
        /// <value>The station design.</value>
        public StationDesign StationDesign => BuildDesign as StationDesign;


        /// <summary>
        /// Gets the description of the station under construction.
        /// </summary>
        /// <value>The description.</value>
        public override string Description => ResourceManager.GetString(BuildDesign.Name);

        public bool HasDuraniumShortage
        {
            get
            {
                GameLog.Client.Test.DebugFormat("ID {0} stationdesign {1}", _productionCenterId, StationDesign.Description);
                return GetFlag(BuildProjectFlags.DuraniumShortage);
            }
            protected set
            {
                SetFlag(BuildProjectFlags.DuraniumShortage, value);
                GameContext.Current.CivilizationManagers[ProductionCenter.Owner.CivID].SitRepEntries
                    .Add(new BuildProjectResourceShortageSitRepEntry(ProductionCenter.Owner, "Duranium", " unknown amount of ", Description));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StationBuildProject"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="design">The design.</param>
        public StationBuildProject(IProductionCenter source, StationDesign design)
            : base(source.Owner, source, design)
        {
            _productionCenterId = source.ObjectID;
            GameLog.Core.Stations.DebugFormat("ID {0} builds {2} at {1}", source.ObjectID, source.Location.ToString(), design.Key);
        }

        /// <summary>
        /// Gets the amount of industry available for investment during the current turn.
        /// </summary>
        /// <returns>The industry available.</returns>
        protected override int GetIndustryAvailable()
        {
            return ProductionCenter.GetBuildOutput(0);
        }

        /// <summary>
        /// Creates an equivalent clone of this <see cref="StationBuildProject"/>.
        /// </summary>
        /// <returns>The clone.</returns>
        public override BuildProject CloneEquivalent()
        {
            return new StationBuildProject(ProductionCenter, StationDesign);
        }

        public override void SerializeOwnedData(IO.Serialization.SerializationWriter writer, object context)
        {
            base.SerializeOwnedData(writer, context);

            writer.Write(_productionCenterId);
        }

        public override void DeserializeOwnedData(IO.Serialization.SerializationReader reader, object context)
        {
            base.DeserializeOwnedData(reader, context);

            _productionCenterId = reader.ReadInt32();
        }
    }
}
